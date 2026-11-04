using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Data.Sqlite;
using bibliotecaserver2;

namespace bibliotecaserver2;

public sealed class Program
{
    private const string ConnectionString = "Data Source=biblioteca.db";
    private const int Port = 5000;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static void Main(string[] args)
    {
        InizializzaDatabase();
        AvviaServer().GetAwaiter().GetResult();
    }

    private static async Task AvviaServer()
    {
        var listener = new TcpListener(IPAddress.Any, Port);
        listener.Start();
        Console.WriteLine($"Server biblioteca in ascolto sulla porta {Port}.");

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
            Console.WriteLine("\nArresto del server in corso...");
        };

        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync(cts.Token);
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await GestisciClientAsync(client, cts.Token);
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Errore con un client: {ex.Message}");
                    }
                }, cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Server arrestato.");
        }
        finally
        {
            listener.Stop();
            Console.WriteLine("Server terminato.");
        }
    }

    private static async Task GestisciClientAsync(TcpClient client, CancellationToken ct)
    {
        var endPoint = client.Client.RemoteEndPoint;
        Console.WriteLine($"Client connesso: {endPoint}");

        using (client)
        using (NetworkStream stream = client.GetStream())
        using (var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true))
        using (var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true) { AutoFlush = true })
        {
            await writer.WriteLineAsync("Benvenuto nel server biblioteca. Invia un oggetto JSON per riga.");
            await writer.WriteLineAsync("Operazioni: cerca, prestito, restituzione, stato");

            string? linea;
            while ((linea = await reader.ReadLineAsync(ct)) is not null)
            {
                string risposta = EseguiOperazione(linea);
                await writer.WriteLineAsync(risposta);
            }
        }

        Console.WriteLine($"Client disconnesso: {endPoint}");
    }

    private static string EseguiOperazione(string linea)
    {
        try
        {
            var req = JsonSerializer.Deserialize<Richiesta>(linea, JsonOptions);
            if (req is null)
                return RispostaErrata("Richiesta non valida");

            return req.Op switch
            {
                "cerca" => Cerca(req.Q),
                "prestito" => Presta(req),
                "restituzione" => Restituisci(req),
                "stato" => StatoUtente(req.Utente),
                null or "" => RispostaErrata("Operazione non specificata"),
                _ => RispostaErrata($"Operazione sconosciuta: {req.Op}")
            };
        }
        catch (JsonException)
        {
            return RispostaErrata("JSON non valido");
        }
        catch (Exception ex)
        {
            return RispostaErrata(ex.Message);
        }
    }

    private record Richiesta(string? Op, string? Q, string? Utente, int IdLibro);

    private static string Cerca(string? q)
    {
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        if (string.IsNullOrWhiteSpace(q))
        {
            cmd.CommandText = "SELECT Id, Titolo, Autore, Disponibile FROM Libri ORDER BY Id";
        }
        else
        {
            cmd.CommandText = "SELECT Id, Titolo, Autore, Disponibile FROM Libri WHERE Titolo LIKE @q OR Autore LIKE @q ORDER BY Id";
            cmd.Parameters.AddWithValue("@q", "%" + q + "%");
        }

        var libri = new List<object>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            libri.Add(new
            {
                id = reader.GetInt32(0),
                titolo = reader.GetString(1),
                autore = reader.GetString(2),
                disponibile = reader.GetInt32(3) == 1
            });
        }

        return JsonSerializer.Serialize(new { ok = true, operazione = "cerca", libri }, JsonOptions);
    }

    private static string Presta(Richiesta req)
    {
        if (string.IsNullOrWhiteSpace(req.Utente) || req.IdLibro <= 0)
            return RispostaErrata("Utente e idLibro obbligatori per il prestito");

        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;

            cmd.CommandText = "SELECT Disponibile FROM Libri WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", req.IdLibro);
            var disponibile = cmd.ExecuteScalar();
            if (disponibile == null || Convert.ToInt64(disponibile) != 1)
            {
                tx.Rollback();
                return JsonSerializer.Serialize(new { ok = false, operazione = "prestito", esito = "non_disponibile", messaggio = "Libro non disponibile" }, JsonOptions);
            }

            cmd.CommandText = "UPDATE Libri SET Disponibile = 0 WHERE Id = @id";
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"
INSERT INTO Prestiti (IdLibro, Utente, DataRichiesta, Stato)
VALUES (@id, @utente, @data, 'confermato');
SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@utente", req.Utente);
            cmd.Parameters.AddWithValue("@data", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            var idPrestito = (long)cmd.ExecuteScalar()!;

            tx.Commit();

            return JsonSerializer.Serialize(new
            {
                ok = true,
                operazione = "prestito",
                esito = "confermato",
                idPrestito,
                idLibro = req.IdLibro,
                utente = req.Utente
            }, JsonOptions);
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    private static string Restituisci(Richiesta req)
    {
        if (string.IsNullOrWhiteSpace(req.Utente) || req.IdLibro <= 0)
            return RispostaErrata("Utente e idLibro obbligatori per la restituzione");

        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;

            cmd.CommandText = @"
SELECT Id FROM Prestiti
WHERE IdLibro = @id AND Utente = @utente AND Stato = 'confermato'
ORDER BY DataRichiesta DESC
LIMIT 1";
            cmd.Parameters.AddWithValue("@id", req.IdLibro);
            cmd.Parameters.AddWithValue("@utente", req.Utente);
            var idPrestito = cmd.ExecuteScalar();

            if (idPrestito == null)
            {
                tx.Rollback();
                return JsonSerializer.Serialize(new
                {
                    ok = false,
                    operazione = "restituzione",
                    esito = "non_trovato",
                    messaggio = "Nessun prestito attivo trovato per questo utente e libro"
                }, JsonOptions);
            }

            cmd.CommandText = "UPDATE Prestiti SET Stato = 'restituito' WHERE Id = @pid";
            cmd.Parameters.AddWithValue("@pid", idPrestito);
            cmd.ExecuteNonQuery();

            cmd.CommandText = "UPDATE Libri SET Disponibile = 1 WHERE Id = @id";
            cmd.ExecuteNonQuery();

            tx.Commit();

            return JsonSerializer.Serialize(new
            {
                ok = true,
                operazione = "restituzione",
                esito = "completato",
                idPrestito,
                idLibro = req.IdLibro,
                utente = req.Utente
            }, JsonOptions);
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    private static string StatoUtente(string? utente)
    {
        if (string.IsNullOrWhiteSpace(utente))
            return RispostaErrata("Utente obbligatorio per consultare lo stato");

        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
SELECT Id, IdLibro, Utente, DataRichiesta, Stato
FROM Prestiti
WHERE Utente = @utente
ORDER BY DataRichiesta DESC";
        cmd.Parameters.AddWithValue("@utente", utente);

        var prestiti = new List<object>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            prestiti.Add(new
            {
                id = reader.GetInt64(0),
                idLibro = reader.GetInt32(1),
                utentePrestito = reader.GetString(2),
                dataRichiesta = reader.GetString(3),
                stato = reader.GetString(4)
            });
        }

        return JsonSerializer.Serialize(new { ok = true, operazione = "stato", utente, prestiti }, JsonOptions);
    }

    private static string RispostaErrata(string messaggio)
    {
        return JsonSerializer.Serialize(new { ok = false, errore = messaggio });
    }

    private static void InizializzaDatabase()
    {
        using var init = new SqliteConnection(ConnectionString);
        init.Open();
        using var cmd = init.CreateCommand();
        cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Libri (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Titolo TEXT NOT NULL,
    Autore TEXT NOT NULL,
    Disponibile INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS Prestiti (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    IdLibro INTEGER NOT NULL,
    Utente TEXT NOT NULL,
    DataRichiesta TEXT NOT NULL,
    Stato TEXT NOT NULL DEFAULT 'richiesto',
    FOREIGN KEY(IdLibro) REFERENCES Libri(Id)
);";
        cmd.ExecuteNonQuery();

        using var countCmd = init.CreateCommand();
        countCmd.CommandText = "SELECT COUNT(*) FROM Libri";
        var totale = (long)countCmd.ExecuteScalar()!;
        if (totale == 0)
        {
            using var seed = init.CreateCommand();
            seed.CommandText = @"
INSERT INTO Libri (Titolo, Autore, Disponibile) VALUES
('Divina Commedia', 'Dante Alighieri', 1),
('I Promessi Sposi', 'Alessandro Manzoni', 1),
('1984', 'George Orwell', 1),
('Il Nome della Rosa', 'Umberto Eco', 1),
('Harry Potter e la Pietra Filosofale', 'J.K. Rowling', 1);";
            seed.ExecuteNonQuery();
        }
    }
}
