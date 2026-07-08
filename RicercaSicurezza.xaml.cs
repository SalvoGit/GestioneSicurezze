using GestioneSicurezze.Models;
using GestioneSicurezze.MultiAwb;
using PdfiumViewer;
using System.Drawing.Printing;
using System.Windows;

namespace GestioneSicurezze
{
    /// <summary>
    /// Logica di interazione per RicercaSicurezza.xaml
    /// </summary>
    public partial class RicercaSicurezza : Window
    {
        ModelloXray modelloXray = new ModelloXray();
        UserSettings _userSettings;
        public RicercaSicurezza()
        {
            InitializeComponent();
            LoadSettings();
            txtRicercaSicurezza.Focus();
        }
        private void LoadSettings()
        {
            _userSettings = UtilitySettings.ReadActualSettings();
        }
        private void btnCercaSicurezza_Click(object sender, RoutedEventArgs e)
        {
            if(String.IsNullOrEmpty(txtRicercaSicurezza.Text))
            {
                MessageBox.Show("Inserire una stringa di ricerca valida","Ricerca Sicurezza - Errore",MessageBoxButton.OK,MessageBoxImage.Error);
                return;
            }

            // Restituzione della riga singola
            //modelloXray = DbOperation.GetSicurezzaByAWB((txtRicercaSicurezza.Text));
            /*if (modelloXray != null)
            {
                SetCampi(modelloXray);
            }
            else
            {
                if (String.IsNullOrEmpty(DbOperation.GetErrorMessage))
                {
                    MessageBox.Show("Nessun risultato trovato", "Ricerca Sicurezza - Nessun Risultato", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Errore durante la ricerca: {DbOperation.GetErrorMessage}", "Ricerca Sicurezza - Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }*/

            //Restituzione di una lista di righe
            List<ModelloXray> listaSicurezze = DbOperation.GetListSicurezzeByAWB(txtRicercaSicurezza.Text.ToUpper());

            if (listaSicurezze != null && listaSicurezze.Count == 1)
            {
                SetCampi(listaSicurezze[0]);
                modelloXray = listaSicurezze[0];
            }
            else if (listaSicurezze.Count > 1)
            {
                SelezionaAwbMultipli selezionaAwbMultipli = new SelezionaAwbMultipli();
                selezionaAwbMultipli.SetAwbList(listaSicurezze);
                selezionaAwbMultipli.ShowDialog();
                ModelloXray? selectedItem = selezionaAwbMultipli.GetSelectedItem();
                SetCampi(selectedItem);
                modelloXray = selectedItem;
            }
            else
            {
                if (String.IsNullOrEmpty(DbOperation.GetErrorMessage))
                {
                    MessageBox.Show("Nessun risultato trovato", "Ricerca Sicurezza - Nessun Risultato", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Errore durante la ricerca: {DbOperation.GetErrorMessage}", "Ricerca Sicurezza - Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SetCampi(ModelloXray modelloXray)
        {
            txtProgressivo.Text = modelloXray.Progressivo.ToString();
            txtCodiceEU.Text = modelloXray.CodiceEnac;
            txtOperatori.Text = modelloXray.Operatore;
            txtCliente.Text = modelloXray.Cliente;
            txtEntrata.Text = modelloXray.NrEntrata;
            txtAwb.Text = modelloXray.Awb;
            txtColli.Text = modelloXray.Colli;
            txtPeso.Text = modelloXray.Peso;
            txtDestinazione.Text = modelloXray.Destinazione;
            txtContenuto.Text = modelloXray.Contenuto;
            txtRiferimento.Text = modelloXray.Riferimento;
            txtRagSocTrasp.Text = modelloXray.Trasportatore;
            txtTarghe.Text = modelloXray.Targa;
            txtSigilloNumero.Text = modelloXray.SigilloNumero;
            txtAutista.Text = modelloXray.Autista;
            txtXray.Text = modelloXray.XRAY ? "SI" : "NO";
            txtQtXray.Text = modelloXray.QT_XRAY.ToString();            
            txtEtd.Text = modelloXray.ETD ? "SI" : "NO";
            txtQtEtd.Text = modelloXray.QT_ETD.ToString();
            txtPhs.Text = modelloXray.PHS ? "SI" : "NO";
            txtVck.Text = modelloXray.VCK ? "SI" : "NO";
            if(modelloXray.STATOMERCE == "SPX")
            {
                txtSpx.Text = "SPX";
            }
            else if(modelloXray.STATOMERCE == "SHR")
            {
                txtSpx.Text = "SHR";
            }            
            txtDataOra.Text = modelloXray.DataEsecuzione.ToString("dd/MM/yyyy HH:mm:ss");
            txtAeroporto.Text = modelloXray.AeroportoDest ?? String.Empty;
        }

        private void RistampaButton_Click(object sender, RoutedEventArgs e)
        {
            if(String.IsNullOrEmpty(txtRicercaSicurezza.Text))
            {
                MessageBox.Show("Inserire un codice AWB valido per la ricerca", "Ristampa - Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {                
                string nomeFile = $"{modelloXray.Progressivo}_{modelloXray.Cliente.Replace(" ", "_")}_{modelloXray.Awb}.pdf";
                string percorsoFile = System.IO.Path.Combine(_userSettings.SavePath, nomeFile);
                if (!System.IO.File.Exists(percorsoFile))
                {
                    MessageBox.Show($"Il file PDF da stampare non esiste: {percorsoFile}", "Ristampa - Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                string printerName = _userSettings.UserPrinter;
                if (PrinterSettings.InstalledPrinters.Cast<string>().Contains(printerName))
                {
                    using (var document = PdfDocument.Load(percorsoFile))
                    {
                        using (var printDocument = document.CreatePrintDocument())
                        {
                            printDocument.PrinterSettings.PrinterName = printerName;
                            printDocument.PrinterSettings.Copies = (short)_userSettings.PrintCopy;
                            printDocument.PrinterSettings.Duplex = Duplex.Simplex;
                            printDocument.PrinterSettings.PrintToFile = false;
                            printDocument.Print();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante la stampa: {ex.Message}", "Ristampa - Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }
        private void ModificaButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(txtRicercaSicurezza.Text))
            {
                MessageBox.Show("Inserire un codice AWB valido per la ricerca", "Ristampa - Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ModificaEntrate modificaEntrate = new ModificaEntrate(modelloXray, _userSettings);
            modificaEntrate.ShowDialog();
            //modelloXray = DbOperation.GetSicurezzaByAWB((txtRicercaSicurezza.Text));
            modelloXray = DbOperation.GetSicurezzaByAWBandID((txtRicercaSicurezza.Text), modelloXray.ID);
            SetCampi(modelloXray);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
