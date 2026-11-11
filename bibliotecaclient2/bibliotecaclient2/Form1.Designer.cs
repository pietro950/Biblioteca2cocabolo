namespace bibliotecaclient2
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblUtente;
        private System.Windows.Forms.TextBox txtUtente;
        private System.Windows.Forms.Label lblRicerca;
        private System.Windows.Forms.TextBox txtRicerca;
        private System.Windows.Forms.Button btnConnetti;
        private System.Windows.Forms.Button btnDisconnetti;
        private System.Windows.Forms.Button btnCerca;
        private System.Windows.Forms.Button btnPresta;
        private System.Windows.Forms.Button btnRestituisci;
        private System.Windows.Forms.Button btnStato;
        private System.Windows.Forms.Label lblStato;
        private System.Windows.Forms.Label lblLibri;
        private System.Windows.Forms.Label lblPrestiti;
        private System.Windows.Forms.DataGridView dgvLibri;
        private System.Windows.Forms.DataGridView dgvPrestiti;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            lblUtente = new System.Windows.Forms.Label();
            txtUtente = new System.Windows.Forms.TextBox();
            lblRicerca = new System.Windows.Forms.Label();
            txtRicerca = new System.Windows.Forms.TextBox();
            btnConnetti = new System.Windows.Forms.Button();
            btnDisconnetti = new System.Windows.Forms.Button();
            btnCerca = new System.Windows.Forms.Button();
            btnPresta = new System.Windows.Forms.Button();
            btnRestituisci = new System.Windows.Forms.Button();
            btnStato = new System.Windows.Forms.Button();
            lblStato = new System.Windows.Forms.Label();
            lblLibri = new System.Windows.Forms.Label();
            lblPrestiti = new System.Windows.Forms.Label();
            dgvLibri = new System.Windows.Forms.DataGridView();
            dgvPrestiti = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(dgvLibri)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(dgvPrestiti)).BeginInit();
            SuspendLayout();

            lblUtente.AutoSize = true;
            lblUtente.Location = new System.Drawing.Point(20, 20);
            lblUtente.Text = "utente:";
            txtUtente.Location = new System.Drawing.Point(75, 17);
            txtUtente.Size = new System.Drawing.Size(180, 23);

            lblRicerca.AutoSize = true;
            lblRicerca.Location = new System.Drawing.Point(280, 20);
            lblRicerca.Text = "cerca:";
            txtRicerca.Location = new System.Drawing.Point(330, 17);
            txtRicerca.Size = new System.Drawing.Size(220, 23);
            txtRicerca.KeyDown += txtRicerca_KeyDown;

            btnConnetti.Location = new System.Drawing.Point(570, 15);
            btnConnetti.Size = new System.Drawing.Size(95, 28);
            btnConnetti.Text = "connetti";
            btnConnetti.Click += btnConnetti_Click;

            btnDisconnetti.Enabled = false;
            btnDisconnetti.Location = new System.Drawing.Point(675, 15);
            btnDisconnetti.Size = new System.Drawing.Size(100, 28);
            btnDisconnetti.Text = "disconnetti";
            btnDisconnetti.Click += btnDisconnetti_Click;

            btnCerca.Enabled = false;
            btnCerca.Location = new System.Drawing.Point(20, 55);
            btnCerca.Size = new System.Drawing.Size(95, 30);
            btnCerca.Text = "cerca libri";
            btnCerca.Click += btnCerca_Click;

            btnPresta.Enabled = false;
            btnPresta.Location = new System.Drawing.Point(125, 55);
            btnPresta.Size = new System.Drawing.Size(110, 30);
            btnPresta.Text = "richiedi prestito";
            btnPresta.Click += btnPresta_Click;

            btnRestituisci.Enabled = false;
            btnRestituisci.Location = new System.Drawing.Point(245, 55);
            btnRestituisci.Size = new System.Drawing.Size(105, 30);
            btnRestituisci.Text = "restituisci";
            btnRestituisci.Click += btnRestituisci_Click;

            btnStato.Enabled = false;
            btnStato.Location = new System.Drawing.Point(360, 55);
            btnStato.Size = new System.Drawing.Size(110, 30);
            btnStato.Text = "vedi prestiti";
            btnStato.Click += btnStato_Click;

            lblStato.AutoSize = true;
            lblStato.Location = new System.Drawing.Point(500, 64);
            lblStato.Text = "disconnesso";

            lblLibri.AutoSize = true;
            lblLibri.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            lblLibri.Location = new System.Drawing.Point(20, 105);
            lblLibri.Text = "libri";

            dgvLibri.AllowUserToAddRows = false;
            dgvLibri.AllowUserToDeleteRows = false;
            dgvLibri.AutoGenerateColumns = true;
            dgvLibri.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dgvLibri.Location = new System.Drawing.Point(20, 130);
            dgvLibri.MultiSelect = false;
            dgvLibri.ReadOnly = true;
            dgvLibri.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvLibri.Size = new System.Drawing.Size(755, 180);

            lblPrestiti.AutoSize = true;
            lblPrestiti.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            lblPrestiti.Location = new System.Drawing.Point(20, 330);
            lblPrestiti.Text = "i miei prestiti";

            dgvPrestiti.AllowUserToAddRows = false;
            dgvPrestiti.AllowUserToDeleteRows = false;
            dgvPrestiti.AutoGenerateColumns = true;
            dgvPrestiti.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dgvPrestiti.Location = new System.Drawing.Point(20, 355);
            dgvPrestiti.ReadOnly = true;
            dgvPrestiti.Size = new System.Drawing.Size(755, 150);

            ClientSize = new System.Drawing.Size(800, 530);
            Controls.Add(lblUtente);
            Controls.Add(txtUtente);
            Controls.Add(lblRicerca);
            Controls.Add(txtRicerca);
            Controls.Add(btnConnetti);
            Controls.Add(btnDisconnetti);
            Controls.Add(btnCerca);
            Controls.Add(btnPresta);
            Controls.Add(btnRestituisci);
            Controls.Add(btnStato);
            Controls.Add(lblStato);
            Controls.Add(lblLibri);
            Controls.Add(dgvLibri);
            Controls.Add(lblPrestiti);
            Controls.Add(dgvPrestiti);
            MinimumSize = new System.Drawing.Size(816, 569);
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "biblioteca client";
            ((System.ComponentModel.ISupportInitialize)(dgvLibri)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(dgvPrestiti)).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private async void txtRicerca_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Enter && btnCerca.Enabled)
            {
                e.SuppressKeyPress = true;
                await EseguiRicercaAsync();
            }
        }
    }
}
