using System.Collections.Generic;

namespace bibliotecaclient2
{
    public sealed class LibroViewModel
    {
        public int id { get; set; }
        public string titolo { get; set; }
        public string autore { get; set; }
        public bool disponibile { get; set; }
    }

    public sealed class PrestitoViewModel
    {
        public long id { get; set; }
        public int idLibro { get; set; }
        public string utentePrestito { get; set; }
        public string dataRichiesta { get; set; }
        public string stato { get; set; }
    }

    public sealed class CercaResponse
    {
        public bool ok { get; set; }
        public string operazione { get; set; }
        public List<LibroViewModel> libri { get; set; }
        public string errore { get; set; }
    }

    public sealed class StatoResponse
    {
        public bool ok { get; set; }
        public string operazione { get; set; }
        public string utente { get; set; }
        public List<PrestitoViewModel> prestiti { get; set; }
        public string errore { get; set; }
    }

    public sealed class OperationResponse
    {
        public bool ok { get; set; }
        public string operazione { get; set; }
        public string esito { get; set; }
        public string messaggio { get; set; }
        public string errore { get; set; }
    }
}
