namespace bibliotecaserver2;

public sealed class Prestito
{
    public int IdLibro { get; set; }

    public string Utente { get; set; } = string.Empty;

    public DateTime DataRichiesta { get; set; } = DateTime.Now;

    public string Stato { get; set; } = "richiesto";
}
