using GestioneSicurezze.Models;
using PdfiumViewer;
using QuestPDF.Fluent;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace GestioneSicurezze
{
    /// <summary>
    /// Logica di interazione per ModificaEntrate.xaml
    /// </summary>
    public partial class ModificaEntrate : Window
    {
        private ModelloXray _modelloXray;
        private readonly UserSettings _userSettings;
        private IReadOnlyList<CodiciEnac> _codiciEnac;
        private IReadOnlyList<CodiciOperatori> _codiciOperatori;
        private IReadOnlyList<SicurClienteCliente> _codiciClienti;
        ModelloXray modelloXray = new ModelloXray();
        public ModificaEntrate(ModelloXray modelloXray, UserSettings userSettings)
        {
            InitializeComponent();
            _modelloXray = modelloXray;
            _userSettings = userSettings;
            SetData();
        }

        private void SetData()
        {
            txtProgressivo.Text = _modelloXray.Progressivo.ToString();
            LoadCodiciEnac();
            LoadOperatori();
            LoadClienti();
            ComboEnac.SelectedValue = _modelloXray.CodiceEnac;
            ComboCliente.SelectedValue = _modelloXray.Cliente;
            ComboOperatori.SelectedValue = _modelloXray.Operatore;
            txtEntrata.Text = _modelloXray.NrEntrata ?? String.Empty;
            txtAwb.Text = _modelloXray.Awb;
            txtColli.Text = _modelloXray.Colli;
            txtPeso.Text = _modelloXray.Peso;
            txtDestinazione.Text = _modelloXray.Destinazione;
            txtContenuto.Text = _modelloXray.Contenuto;
            txtRiferimento.Text = _modelloXray.Riferimento ?? String.Empty;
            txtSigilloNumero.Text = _modelloXray.SigilloNumero ?? String.Empty;
            txtRagSocTrasp.Text = _modelloXray.Trasportatore ?? String.Empty;
            txtAutista.Text = _modelloXray.Autista ?? String.Empty;
            txtTarghe.Text = _modelloXray.Targa ?? String.Empty;
            chXray.IsChecked = _modelloXray.XRAY;
            txtQtXray.Text = _modelloXray.QT_XRAY.ToString();
            txtQtEtd.Text = _modelloXray.QT_ETD.ToString();
            chEtd.IsChecked = _modelloXray.ETD;
            chPhs.IsChecked = _modelloXray.PHS;
            chVck.IsChecked = _modelloXray.VCK;
            if(_modelloXray.STATOMERCE == "SPX")
            {
                rbSpx.IsChecked = true;
                rbShr.IsChecked = false;
            }
            else if(_modelloXray.STATOMERCE == "SHR")
            {
                rbSpx.IsChecked = false;
                rbShr.IsChecked = true;
            }            
            txtDataOra.Text = _modelloXray.DataEsecuzione.ToString();
            txtAeroporto.Text = _modelloXray.AeroportoDest;
        }

        private void LoadCodiciEnac()
        {
            ComboEnac.ItemsSource = null;
            _codiciEnac = DbOperation.CodiciEnac();
            if (_codiciEnac == null || _codiciEnac.Count == 0)
            {
                MessageBox.Show("Nessun Codice Enac trovato nel database.\n\nContattare il servizio IT.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            ComboEnac.ItemsSource = _codiciEnac;
            ComboEnac.SelectedIndex = 0;
        }

        private void LoadClienti()
        {
            ComboCliente.ItemsSource = null;
            _codiciClienti = DbOperation.NominativiClienti();
            if (_codiciClienti == null || _codiciClienti.Count == 0)
            {
                MessageBox.Show("Nessun Cliente trovato nel database.\n\nContattare il servizio IT.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            ComboCliente.ItemsSource = _codiciClienti;
            ComboCliente.SelectedIndex = 0;
        }
        private void LoadOperatori()
        {
            ComboOperatori.ItemsSource = null;
            _codiciOperatori = DbOperation.CodiciOperatori();
            if (_codiciOperatori == null || _codiciOperatori.Count == 0)
            {
                MessageBox.Show("Nessun Codice Operatore trovato nel database.\n\nContattare il servizio IT.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            ComboOperatori.ItemsSource = _codiciOperatori;
            ComboOperatori.SelectedIndex = 0;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            ChiudiFinistra();
        }

        private void ChiudiFinistra()
        {
            this.Close();
        }

        private void UpdateCloseButton_Click(object sender, RoutedEventArgs e)
        {
            AggiornaDati();
            AggiornaSicurezza();
            ChiudiFinistra();
        }

        private bool AggiornaDati()
        {            
            modelloXray.ID = _modelloXray.ID;
            modelloXray.Progressivo = _modelloXray.Progressivo;
            modelloXray.NrEntrata = txtEntrata.Text.ToUpper() ?? string.Empty;
            modelloXray.CodiceEnac = ComboEnac.SelectedValue?.ToString() ?? string.Empty;
            modelloXray.Operatore = ComboOperatori.SelectedValue?.ToString() ?? string.Empty;
            modelloXray.Cliente = ComboCliente.SelectedValue?.ToString() ?? string.Empty;
            modelloXray.Awb = txtAwb.Text.ToUpper();
            modelloXray.Colli = txtColli.Text.ToUpper();
            modelloXray.Peso = txtPeso.Text.ToUpper();
            modelloXray.Destinazione = txtDestinazione.Text.ToUpper();
            modelloXray.Contenuto = txtContenuto.Text.ToUpper();
            modelloXray.Riferimento = txtRiferimento.Text.ToUpper() ?? string.Empty;
            modelloXray.Trasportatore = txtRagSocTrasp.Text.ToUpper() ?? string.Empty;
            modelloXray.Targa = txtTarghe.Text.ToUpper() ?? string.Empty;
            modelloXray.SigilloNumero = txtSigilloNumero.Text.ToUpper() ?? string.Empty;
            modelloXray.Autista = txtAutista.Text.ToUpper() ?? string.Empty;
            modelloXray.XRAY = chXray.IsChecked ?? false;
            modelloXray.ETD = chEtd.IsChecked ?? false;
            modelloXray.PHS = chPhs.IsChecked ?? false;
            modelloXray.VCK = chVck.IsChecked ?? false;
            if (modelloXray.XRAY)
            {
                int qtXray = int.TryParse(txtQtXray.Text, out int result) ? result : 1;
                modelloXray.QT_XRAY = qtXray;
            }
            else
            {
                modelloXray.QT_XRAY = 0;
            }

            if (modelloXray.ETD)
            {
                int qtetd = int.TryParse(txtQtEtd.Text, out int resultEtd) ? resultEtd : 1;
                modelloXray.QT_ETD = qtetd;
            }
            else
            {
                modelloXray.QT_ETD = 0;
            }
            if (rbSpx.IsChecked == true)
            {
                modelloXray.STATOMERCE = "SPX";
            }
            else if(rbShr.IsChecked == true)
            {
                modelloXray.STATOMERCE = "SHR";
            }            
            modelloXray.DataEsecuzione = _modelloXray.DataEsecuzione;
            modelloXray.AeroportoDest = txtAeroporto.Text.ToUpper() ?? string.Empty;

            return VerificaInput(modelloXray);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (AggiornaDati())
            {
                SalvaNuovoPDF();
                AggiornaSicurezza();
            }
            else
            {
                MessageBox.Show("Correggere gli errori prima di eseguire il salvataggio", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SalvaNuovoPDF()
        {
            CreazioneXrayDeclaration creazioneXray = new CreazioneXrayDeclaration(modelloXray);
            string nomeFile = $"{modelloXray.Progressivo}_{modelloXray.Cliente.Replace(" ", "_")}_{modelloXray.Awb}.pdf";
            string filePath = Path.Combine(_userSettings.SavePath, nomeFile);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            creazioneXray.GeneratePdf(Path.Combine(_userSettings.SavePath, nomeFile));

            StampaFile(filePath);
        }

        private void AggiornaSicurezza()
        {
            try
            {
                int righeAggiornate = DbOperation.UpdateSicurezzaDB(modelloXray);
                switch (righeAggiornate)
                {
                    case 0:
                        MessageBox.Show("Nessun dato aggiornato", "INFO", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                     case 1:
                        MessageBox.Show("Dati aggiornati correttamente", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                    case -1:
                        MessageBox.Show(DbOperation.GetErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il salvataggio: {DbOperation.GetErrorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool VerificaInput(ModelloXray modelloXray)
        {
            var validatore = new ApplicationValidator();
            var result = validatore.Validate(modelloXray);

            if (!result.IsValid)
            {
                foreach (var item in result.Errors)
                {
                    switch (item.PropertyName)
                    {
                        case nameof(ModelloXray.CodiceEnac):
                            lblCodEU.Content = item.ErrorMessage;
                            break;
                        case nameof(ModelloXray.Operatore):
                            lblOperatore.Content = item.ErrorMessage;
                            break;
                        case nameof(ModelloXray.Cliente):
                            lblCliente.Content = item.ErrorMessage;
                            ComboCliente.BorderBrush = System.Windows.Media.Brushes.Red;
                            break;
                        case nameof(ModelloXray.Awb):
                            lblAWB.Content = item.ErrorMessage;
                            txtAwb.BorderBrush = System.Windows.Media.Brushes.Red;
                            break;
                        case nameof(ModelloXray.Colli):
                            lblColli.Content = item.ErrorMessage;
                            txtColli.BorderBrush = System.Windows.Media.Brushes.Red;
                            break;
                        case nameof(ModelloXray.Peso):
                            lblPeso.Content = item.ErrorMessage;
                            txtPeso.BorderBrush = System.Windows.Media.Brushes.Red;
                            break;
                        case nameof(ModelloXray.Destinazione):
                            lblDestinazione.Content = item.ErrorMessage;
                            txtDestinazione.BorderBrush = System.Windows.Media.Brushes.Red;
                            break;
                        case nameof(ModelloXray.Contenuto):
                            lblContenuto.Content = item.ErrorMessage;
                            txtContenuto.BorderBrush = System.Windows.Media.Brushes.Red;
                            break;
                    }
                }

                MessageBox.Show("Prima di poter effettuare il salvataggio è necessario correggere gli errori evidenziati", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Stop);
                return false;
            }

            return true;
        }

        private void RistampaButton_Click(object sender, RoutedEventArgs e)
        {
            string nomeFile = $"{_modelloXray.Progressivo}_{_modelloXray.Cliente.Replace(" ", "_")}_{_modelloXray.Awb}.pdf";
            string filePath = Path.Combine(_userSettings.SavePath, nomeFile);
            if (!File.Exists(filePath))
            {
                /*CreazioneXrayDeclaration creazioneXray = new CreazioneXrayDeclaration(_modelloXray);
                creazioneXray.GeneratePdf(Path.Combine(_userSettings.SavePath, nomeFile));*/
                MessageBox.Show("Il file PDF da stampare non è stato trovato.\n\nControllare che il file esista o che il percorso sia corretto.", "File Non Trovato", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }            
            
            StampaFile(filePath);
        }
        private void StampaFile(string fileDaStampare)
        {
            if (_userSettings.PrintSecur)
            {
                string printerName = _userSettings.UserPrinter;
                if (PrinterSettings.InstalledPrinters.Cast<string>().Contains(printerName))
                {
                    using (var document = PdfDocument.Load(fileDaStampare))
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
        }
    }
}
