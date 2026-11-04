using Microsoft.Data.Sqlite;
using bibliotecaserver2;

namespace bibliotecaserver2;

public sealed class Program
{
    private const string ConnectionString = "Data Source=biblioteca.db";

    private static void Main(string[] args)
    {
        InizializzaDatabase();
        Console.WriteLine("Database SQLite inizializzato.");
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
