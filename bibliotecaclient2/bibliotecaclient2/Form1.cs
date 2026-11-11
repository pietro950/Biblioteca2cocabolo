using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace bibliotecaclient2
{
    public partial class Form1 : Form
    {
        private const string ServerHost = "127.0.0.1";
        private const int ServerPort = 5000;

        private readonly BibliotecaClient bibliotecaClient;
        private readonly JavaScriptSerializer json = new JavaScriptSerializer();
        private List<LibroViewModel> libri = new List<LibroViewModel>();

        public Form1()
        {
            InitializeComponent();
            bibliotecaClient = new BibliotecaClient(ServerHost, ServerPort);
            FormClosing += Form1_FormClosing;
        }

        private async void btnConnetti_Click(object sender, EventArgs e)
        {
            try
            {
                await bibliotecaClient.ConnectAsync();
                lblStato.Text = "connesso al server";
                btnConnetti.Enabled = false;
                btnDisconnetti.Enabled = true;
                SetOperazioniEnabled(true);
            }
            catch (Exception ex)
            {
                MostraErrore("Impossibile connettersi al server: " + ex.Message);
            }
        }

        private void btnDisconnetti_Click(object sender, EventArgs e)
        {
            bibliotecaClient.Disconnect();
            lblStato.Text = "disconnesso";
            btnConnetti.Enabled = true;
            btnDisconnetti.Enabled = false;
            SetOperazioniEnabled(false);
        }

        private async void btnCerca_Click(object sender, EventArgs e)
        {
            await EseguiRicercaAsync();
        }

        private async void btnPresta_Click(object sender, EventArgs e)
        {
            LibroViewModel libro = LibroSelezionato();
            if (libro == null)
                return;

            if (!UtenteValido())
                return;

            string richiesta = json.Serialize(new
            {
                op = "prestito",
                idLibro = libro.id,
                utente = txtUtente.Text.Trim()
            });

            await EseguiOperazioneAsync(richiesta, "prestito");
        }

        private async void btnRestituisci_Click(object sender, EventArgs e)
        {
            LibroViewModel libro = LibroSelezionato();
            if (libro == null)
                return;

            if (!UtenteValido())
                return;

            string richiesta = json.Serialize(new
            {
                op = "restituzione",
                idLibro = libro.id,
                utente = txtUtente.Text.Trim()
            });

            await EseguiOperazioneAsync(richiesta, "restituzione");
        }

        private async void btnStato_Click(object sender, EventArgs e)
        {
            if (!UtenteValido())
                return;

            string richiesta = json.Serialize(new
            {
                op = "stato",
                utente = txtUtente.Text.Trim()
            });

            try
            {
                string risposta = await bibliotecaClient.SendAsync(richiesta);
                StatoResponse stato = json.Deserialize<StatoResponse>(risposta);

                if (!stato.ok)
                {
                    MostraErrore(stato.errore ?? "Errore durante la lettura dello stato.");
                    return;
                }

                dgvPrestiti.DataSource = stato.prestiti ?? new List<PrestitoViewModel>();
                lblStato.Text = "stato prestiti aggiornato";
            }
            catch (Exception ex)
            {
                GestisciErroreConnessione(ex);
            }
        }

        private async Task EseguiRicercaAsync()
        {
            try
            {
                string richiesta = json.Serialize(new
                {
                    op = "cerca",
                    q = txtRicerca.Text.Trim()
                });

                string risposta = await bibliotecaClient.SendAsync(richiesta);
                CercaResponse risultato = json.Deserialize<CercaResponse>(risposta);

                if (!risultato.ok)
                {
                    MostraErrore(risultato.errore ?? "Errore durante la ricerca.");
                    return;
                }

                libri = risultato.libri ?? new List<LibroViewModel>();
                dgvLibri.DataSource = libri;
                lblStato.Text = libri.Count + " libri trovati";
            }
            catch (Exception ex)
            {
                GestisciErroreConnessione(ex);
            }
        }

        private async Task EseguiOperazioneAsync(string richiesta, string operazione)
        {
            try
            {
                string risposta = await bibliotecaClient.SendAsync(richiesta);
                OperationResponse risultato = json.Deserialize<OperationResponse>(risposta);

                if (!risultato.ok)
                {
                    MostraErrore(risultato.messaggio ?? risultato.errore ?? "Operazione non riuscita.");
                    return;
                }

                MessageBox.Show(
                    risultato.messaggio ?? "Operazione completata.",
                    operazione,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                await EseguiRicercaAsync();
                await AggiornaStatoSilenziosamenteAsync();
            }
            catch (Exception ex)
            {
                GestisciErroreConnessione(ex);
            }
        }

        private async Task AggiornaStatoSilenziosamenteAsync()
        {
            if (!UtenteValido(false))
                return;

            string richiesta = json.Serialize(new
            {
                op = "stato",
                utente = txtUtente.Text.Trim()
            });

            string risposta = await bibliotecaClient.SendAsync(richiesta);
            StatoResponse stato = json.Deserialize<StatoResponse>(risposta);
            if (stato.ok)
                dgvPrestiti.DataSource = stato.prestiti ?? new List<PrestitoViewModel>();
        }

        private LibroViewModel LibroSelezionato()
        {
            if (dgvLibri.CurrentRow == null)
            {
                MostraErrore("Seleziona prima un libro.");
                return null;
            }

            return dgvLibri.CurrentRow.DataBoundItem as LibroViewModel;
        }

        private bool UtenteValido(bool mostraErrore = true)
        {
            if (!string.IsNullOrWhiteSpace(txtUtente.Text))
                return true;

            if (mostraErrore)
                MostraErrore("Inserisci il nome utente.");

            return false;
        }

        private void SetOperazioniEnabled(bool enabled)
        {
            txtRicerca.Enabled = enabled;
            txtUtente.Enabled = enabled;
            btnCerca.Enabled = enabled;
            btnPresta.Enabled = enabled;
            btnRestituisci.Enabled = enabled;
            btnStato.Enabled = enabled;
        }

        private void GestisciErroreConnessione(Exception ex)
        {
            lblStato.Text = "errore di comunicazione";
            MostraErrore(ex.Message);
        }

        private void MostraErrore(string messaggio)
        {
            MessageBox.Show(messaggio, "biblioteca", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            bibliotecaClient.Dispose();
        }
    }
}
