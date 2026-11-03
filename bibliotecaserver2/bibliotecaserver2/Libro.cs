namespace bibliotecaserver2;

public sealed class Libro
{
    public int Id { get; set; }

    public string Titolo { get; set; } = string.Empty;

    public string Autore { get; set; } = string.Empty;

    public bool Disponibile { get; set; } = true;
}
