using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using bibliotecaserver2;

namespace bibliotecaserver2;

public sealed class Program
{
    private const string ConnectionString = "Data Source=biblioteca.db";
    private const int Port = 5000;

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
        using (client)
        using (NetworkStream stream = client.GetStream())
        {
            byte[] benvenuto = System.Text.Encoding.UTF8.GetBytes("Connesso al server biblioteca.\r\n");
            await stream.WriteAsync(benvenuto, ct);
        }
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
