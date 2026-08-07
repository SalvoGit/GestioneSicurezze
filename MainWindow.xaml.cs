using GestioneSicurezze.AutistiTarghe;
using GestioneSicurezze.Models;
using GestioneSicurezze.VelopackService;
using PdfiumViewer;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace GestioneSicurezze
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly VelopackUpdateService _updateService;
        private IReadOnlyList<CodiciEnac> _codiciEnac;
        private IReadOnlyList<CodiciOperatori> _codiciOperatori;
        private IReadOnlyList<SicurClienteCliente> _codiciClienti;
        private IReadOnlyList<AutistaTarga> _codiciAutisti;
        private string autistaDaInserire = string.Empty;
        ObservableCollection<ModelloXray> ultimiInserimenti = new ObservableCollection<ModelloXray>();
        UserSettings _userSettings;
        //string SavePath = @"C:\Sviluppo\Sicurezze";
        public MainWindow()
        {            
            InitializeComponent();

            // Inizializza il servizio puntando alla directory o all'URL di rilascio.
            // Sostituisci questo percorso con quello effettivo della tua distribuzione.
            _updateService = new VelopackUpdateService(@"\\192.168.1.5\Applicazioni\Rilascio_Software\Gestione_Sicurezze\Pubblica");

            LoadSettings();            
            QuestPDF.Settings.License = LicenseType.Community;
            lis_UltimiInseriti.ItemsSource = ultimiInserimenti;
            /*********MODIFICA DEL 23/03/2026 PER LA GESTIONE DEL PROGRESSIVO IN CASO DI CONFLICT NEL DB*********/
            //txtProgressivo.Text = DbOperation.GetNextProgressivo().ToString();
            /****************************************************************************/
            if (!_userSettings.Modificabile)
            {
                ComboEnac.IsEnabled = false;
            }
            txtEntrata.Focus();
            //txtAwb.Focus();
            //var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            //var resource = assembly.GetManifestResourceNames();
            //foreach (var item in resource)
            //{
            //    Debug.WriteLine(item);
            //}
        }        
        private void LoadSettings()
        {
            _userSettings = UtilitySettings.ReadActualSettings();
        }
        private void Save(object sender, RoutedEventArgs e)
        {
            var model = _userSettings;
            ModelloXray modelloXray = new ModelloXray();
            modelloXray.NrEntrata = txtEntrata.Text.ToUpper() ?? string.Empty;
            modelloXray.CodiceEnac = ComboEnac.SelectedValue?.ToString() ?? string.Empty;
            modelloXray.Operatore = ComboOperatori.SelectedValue?.ToString() ?? string.Empty;
            modelloXray.Cliente = ComboCliente.SelectedValue?.ToString() ?? string.Empty;
            modelloXray.Awb = txtAwb.Text.ToUpper();
            modelloXray.Colli = txtColli.Text.ToUpper();

            if (txtPeso.Text.Contains(".")) 
            {
                modelloXray.Peso = txtPeso.Text.Replace(".", ",").ToUpper();
            }
            else
            {
                modelloXray.Peso = txtPeso.Text.ToUpper();
            }

             
            modelloXray.Destinazione = txtDestinazione.Text.ToUpper();
            modelloXray.Contenuto = txtContenuto.Text.ToUpper();
            modelloXray.Riferimento = txtRiferimento.Text.ToUpper() ?? string.Empty;
            modelloXray.Trasportatore = txtRagSocTrasp.Text.ToUpper() ?? string.Empty;
            modelloXray.Targa = txtTarghe.Text.ToUpper() ?? string.Empty;
            modelloXray.SigilloNumero = txtSigilloNumero.Text.ToUpper() ?? string.Empty;
            //modelloXray.Autista = txtAutista.Text.ToUpper() ?? string.Empty;
            //modelloXray.Autista = ((AutistaTarga)ComboAutista.SelectedItem)?.Autista ?? string.Empty;
            modelloXray.Autista = autistaDaInserire;
            modelloXray.XRAY = chXray.IsChecked ?? false;
            modelloXray.ETD = chEtd.IsChecked ?? false;
            modelloXray.PHS = chPhs.IsChecked ?? false;
            modelloXray.VCK = chVck.IsChecked ?? false;
            if(rbSpx.IsChecked == true)
            {
                modelloXray.STATOMERCE = "SPX";
            }
            else if(rbShr.IsChecked == true)
            {
                modelloXray.STATOMERCE = "SHR";
            }
            else
            {
                modelloXray.STATOMERCE = string.Empty;
            }

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
            

            modelloXray.DataEsecuzione = DateTime.Now;
            modelloXray.AeroportoDest = txtAeroporto.Text.ToUpper() ?? string.Empty;

            bool continua = VerificaInput(modelloXray);

            if (continua)
            {
                try
                {
                    /************** MODIFICA DEL 23/03/2026 *****************/
                    //modelloXray.Progressivo = DbOperation.GetNextProgressivo();
                    //if (modelloXray.Progressivo == -1)
                    //{
                    //    string messaggioErrore = DbOperation.GetErrorMessage;
                    //    MessageBox.Show($"Errore durante il recupero del Progressivo.\n\nContattare il servizio IT.\n\nErrore: {messaggioErrore}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    //    return;
                    //}
                    /*******************************************************/


                    /**************VERIFICO SE L'AWB E' GIA' STATO INSERITO NELLA MEZZ'ORA PRECEDENTE *****************/
                    MessageBoxResult sceltaOperatore;

                    if(DbOperation.ThirtyMinutesCheck(modelloXray.Awb))
                    {
                        sceltaOperatore = MessageBox.Show($"L'AWB {modelloXray.Awb} è già stato inserito nel database nella mezz'ora precedente.\n\nSi desidera procedere comunque all'inserimento?",
                            "Error", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (sceltaOperatore == MessageBoxResult.No)
                        {
                            return;
                        }
                    }
                    /******************************************************************************************************/

                    /****** ******** MODIFICA DEL 23/03/2026 PER LA GESTIONE DEI CONFLICTS NEL DB *****************/
                    DBResult risultato = DbOperation.InsertSicurezzaDBNoProg(modelloXray);
                    if (risultato.ID == -1)
                    {
                        string messaggioErrore = DbOperation.GetErrorMessage;
                        MessageBox.Show($"Errore durante il salvataggio della sicurezza nel database.\n\nContattare il servizio IT.\n\nErrore: {messaggioErrore}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    modelloXray.ID = risultato.ID;
                    modelloXray.Progressivo = risultato.PROGRESSIVO;
                    /*********************************************************************************************/

                    txtProgressivo.Text = modelloXray.Progressivo.ToString();
                    CreazioneXrayDeclaration creazioneXray = new CreazioneXrayDeclaration(modelloXray);
                    string nomeFile = $"{modelloXray.Progressivo}_{modelloXray.Cliente.Replace(" ", "_")}_{modelloXray.Awb}.pdf";
                    creazioneXray.GeneratePdf(Path.Combine(_userSettings.SavePath, nomeFile));
                    //int idGenerato = SalvaSicurezza(modelloXray);
                    //modelloXray.ID = idGenerato;
                    
                    /******** MODIFICA PER LA GESTIONE DEI CONFLICTS NEL DB ********/
                    //DBResult esitoOperazione = DbOperation.InsertSicurezzaWithConflictDB(modelloXray);
                    //if(esitoOperazione.ID == -1)
                    //{
                    //    string messaggioErrore = DbOperation.GetErrorMessage;
                    //    MessageBox.Show($"Errore durante il salvataggio della sicurezza nel database.\n\nContattare il servizio IT.\n\nErrore: {messaggioErrore}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    //    return;
                    //}
                    //modelloXray.ID = esitoOperazione.ID;
                    //modelloXray.Progressivo = esitoOperazione.PROGRESSIVO;
                    /***************************************************************/

                    StampaFile(Path.Combine(_userSettings.SavePath, nomeFile));

                    CaricaUltimi10(modelloXray.Operatore);
                    ClearCampi();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore durante la creazione del documento PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void OnlyNumbers(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            //accetto solo numeri
            if (char.IsDigit(e.Text, 0))
            {
                return;
            }

            //acetto anche la virgola
            TextBox textBox = sender as TextBox;
            if(e.Text == "," && !textBox.Text.Contains(","))
            {
                return;
            }

            if (e.Text == "." && !textBox.Text.Contains("."))
            {
                return;
            }

            e.Handled = true;
            //e.Handled = !e.Text.All(char.IsDigit);
        }
        private void CaricaUltimi10(string operatore)
        {
            List<ModelloXray> lastInsert = DbOperation.SelectTop10(operatore);
            if (lastInsert.Count == 0)
            {
                MessageBox.Show($"Errore durante il recupero degli ultimi inserimenti.\n\nContattare il servizio IT.\n\nErrore: {DbOperation.GetErrorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ultimiInserimenti.Clear();
            foreach (var itemInList in lastInsert)
            {
                ultimiInserimenti.Add(itemInList);
            }
        }
        private void ClearCampi()
        {
            txtEntrata.Clear();
            txtAwb.Clear();
            txtColli.Clear();
            txtPeso.Clear();
            txtDestinazione.Clear();
            txtContenuto.Text = "VARIE";
            txtRiferimento.Clear();
            txtRagSocTrasp.Clear();
            txtTarghe.Clear();
            txtSigilloNumero.Clear();
            //txtAutista.Clear();
            ComboAutista.SelectedIndex = -1;
            chXray.IsChecked = true;
            chEtd.IsChecked = false;
            chPhs.IsChecked = false;
            chVck.IsChecked = false;
            rbSpx.IsChecked = true;
            rbShr.IsChecked = false;            
            txtAeroporto.Clear();
            /********* MODIFICA DEL 23/03/2026 PER LA GESTIONE DEL PROGRESSIVO IN CASO DI CONFLICT NEL DB*********/
            //txtProgressivo.Text = DbOperation.GetNextProgressivo().ToString();
            /****************************************************************************/
            ClearErrorMessages();
            txtQtEtd.Text = "0";
            txtQtXray.Text = "0";
            //txtAwb.Focus();
            txtEntrata.Focus();
        }
        private int SalvaSicurezza(ModelloXray modelloXray)
        {

            int idGenerato = -1;            
            bool result = DbOperation.OpenConnection();
            if (result)
            {
                idGenerato = DbOperation.InsertSicurezzaDB(modelloXray);                
            }
            else
            {
                MessageBox.Show(DbOperation.GetErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            DbOperation.CloseConnection();
            return idGenerato;
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
                        PdfPrintSettings settings = new PdfPrintSettings(PdfPrintMode.ShrinkToMargin);
                        using (var printDocument = document.CreatePrintDocument(settings))
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
        private void ClearErrorMessages()
        {
            //#FFABADB3
            SolidColorBrush defaultBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFABADB3"));
            txtEntrata.BorderBrush = defaultBrush;
            txtAwb.BorderBrush = defaultBrush;
            txtColli.BorderBrush = defaultBrush;
            txtPeso.BorderBrush = defaultBrush;
            txtDestinazione.BorderBrush = defaultBrush;
            txtContenuto.BorderBrush = defaultBrush;

            lblCodEU.Content = string.Empty;
            lblOperatore.Content = string.Empty;
            lblCliente.Content = string.Empty;
            lblEntrata.Content = string.Empty;
            lblAWB.Content = string.Empty;
            lblColli.Content = string.Empty;
            lblPeso.Content = string.Empty;
            lblDestinazione.Content = string.Empty;
            lblContenuto.Content = string.Empty;
        }
        private bool VerificaInput(ModelloXray modelloXray)
        {
            var validatore = new ApplicationValidator();
            var result = validatore.Validate(modelloXray);
            ClearErrorMessages();

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

                /*if(string.IsNullOrEmpty(txtQtXray.Text))
                {
                    lblQtRayError.Content = "Inserire una quan";
                    ComboEnac.BorderBrush = Brushes.Gray;
                }*/
                MessageBox.Show("Prima di poter effettuare il salvataggio è necessario correggere gli errori evidenziati","Error",
                    MessageBoxButton.OK,MessageBoxImage.Stop);
                return false;
            }

            if(modelloXray.XRAY && modelloXray.ETD)
            {
                int colliTotali = int.TryParse(modelloXray.Colli, out int colli) ? colli : 0;

                if((modelloXray.QT_XRAY == 0) && (modelloXray.QT_ETD == 0))
                {
                    MessageBox.Show("INSERIRE QUANTITA' PER XRAY O ETD", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Stop);
                    return false;
                }

                if ((modelloXray.QT_XRAY + modelloXray.QT_ETD) != colliTotali)
                {
                    MessageBox.Show("SOMMA QUANTITA' XRAY + QUANTITA' ETD DIVERSA DALLA QUANTITA' DI COLLI TOTALI", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Stop);
                    return false;
                }
            }            

            return true;
        }
        private void AggiornaListaEnac(object sender, RoutedEventArgs e)
        {
            LoadCodiciEnac();
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Avvia il controllo e il download in background senza bloccare la UI
            bool hasUpdateReady = await _updateService.PrepareUpdateAsync();
            // Se è stato trovato e scaricato un aggiornamento, rendi operativo il bottone
            //hasUpdateReady = true;
            if (hasUpdateReady)
            {
                UpdateButton.Visibility = Visibility.Visible;
                UpdateButton.IsEnabled = true;
            }

            LoadCodiciEnac();
            LoadOperatori();
            LoadClienti();
            LoadAutisti();
            SetDefaultSettings();
        }
        private void SetDefaultSettings()
        {
            if(!string.IsNullOrEmpty(_userSettings.DefaultClient))
            {
                ComboCliente.SelectedValue = _userSettings.DefaultClient;
            }
            
            ComboOperatori.SelectedValue = _userSettings.DefaultOperator;
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
            SetDefaultEnacCode();
        }
        private void SetDefaultEnacCode()
        {
            if(_userSettings.Modificabile)
            {
                ComboEnac.IsEnabled = true;
            }
            else
            {
                ComboEnac.IsEnabled = false;
            }

            if (!string.IsNullOrEmpty(_userSettings.DefaultEnacCode))
            {
                ComboEnac.SelectedValue = _userSettings.DefaultEnacCode;
                return;
            }

            ComboEnac.SelectedIndex = 0;
        }
        private void AggiornaListaClienti(object sender, RoutedEventArgs e)
        {
            LoadClienti();
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
        private void LoadAutisti()
        {
            ComboAutista.ItemsSource = null;
            _codiciAutisti = DbOperation.GetAutistiTargheRagioniSociali();
            if (_codiciAutisti == null || _codiciAutisti.Count == 0)
            {
                MessageBox.Show("Nessun Autista trovato nel database.\n\nContattare il servizio IT.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            ComboAutista.ItemsSource = _codiciAutisti;
            ComboAutista.SelectedIndex = -1;
        }
        private void AggiornaListaOperatori(object sender, RoutedEventArgs e)
        {
            LoadOperatori();
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
        private void InsertNuovoOperatore(object sender, RoutedEventArgs e)
        {
            NuovoOperatore nuovoOperatoreWindow = new NuovoOperatore();
            nuovoOperatoreWindow.ShowDialog();
            LoadOperatori();
        }
        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            Settings settingsWindow = new Settings();
            settingsWindow.ShowDialog();
            LoadSettings();
            SetDefaultEnacCode();
            SetDefaultSettings();
        }

        private void NuovoClienteButton_Click(object sender, RoutedEventArgs e)
        {
            NuovoCliente nuovoClienteWindow = new NuovoCliente();
            nuovoClienteWindow.ShowDialog();
            LoadClienti();
        }
        private void NuovoAutistaButton_Click(object sender, RoutedEventArgs e)
        {
            NuovoAutista nuovoAutistaWindow = new NuovoAutista();
            nuovoAutistaWindow.ShowDialog();
            LoadAutisti();
        }
        private void Modifica_Click(object sender, RoutedEventArgs e)
        {
            var elemento = (sender as Button).Tag as ModelloXray;           
            ModificaEntrate modificaWindow = new ModificaEntrate(elemento, _userSettings);
            modificaWindow.ShowDialog();
            CaricaUltimi10(elemento.Operatore);
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {            
            try
            {
                var elemento = (sender as Button).Tag as ModelloXray;
                int righeCancellate = DbOperation.DeleteSicurezza(elemento.ID);
                DeletePDF(elemento);
                MessageBox.Show(righeCancellate > 0 ? "Sicurezza cancellata correttamente." : $"Errore durante la cancellazione della sicurezza.\n{DbOperation.GetErrorMessage}",
                    "Success",MessageBoxButton.OK,MessageBoxImage.Information);
                //ultimiInserimenti.Remove(elemento);
                CaricaUltimi10(elemento.Operatore);
            }
            catch (Exception ex) { }
            
        }

        private void DeletePDF(ModelloXray elemento)
        {
            string nomeFile = $"{elemento.Progressivo}_{elemento.Cliente.Replace(" ", "_")}_{elemento.Awb}.pdf";
            string filePath = Path.Combine(_userSettings.SavePath, nomeFile);

            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante la cancellazione del file PDF {nomeFile}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            RicercaSicurezza ricercaWindow = new RicercaSicurezza();
            ricercaWindow.ShowDialog();
        }

        private void btnAddEntrata_Click(object sender, RoutedEventArgs e)
        {
            //ObservableCollection<MultiEntrate> multiEntrateList = new ObservableCollection<MultiEntrate>();
            //MultiEntrate multi = new MultiEntrate();
            InserimentoMultiEntrate inserimentoMultiWindow = new InserimentoMultiEntrate();
            if (inserimentoMultiWindow.ShowDialog() == true)
            {
                var lista = inserimentoMultiWindow.EntrateMultiple;
                if (lista != null && lista.Count > 0)
                {
                    foreach (var item in lista)
                    {
                        txtEntrata.Text = txtEntrata.Text + item.NrEntrata + ";";
                    }
                }
            }
        }

        private void SigilliButton_Click(object sender, RoutedEventArgs e)
        {
            Sigilli sigilli = new Sigilli();
            sigilli.ShowDialog();
        }

        private void ComboAutista_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (ComboAutista.SelectedValue != null && (int)ComboAutista.SelectedValue != -1){
            //    var selectedAutista = _codiciAutisti.FirstOrDefault(a => a.ID == (int)ComboAutista.SelectedValue);
            //    txtTarghe.Text = selectedAutista?.Targa ?? string.Empty;
            //    txtRagSocTrasp.Text = selectedAutista?.RagioneSociale ?? string.Empty;
            //}
            //else
            //{
            //    txtTarghe.Text = string.Empty;
            //    txtRagSocTrasp.Text = string.Empty;
            //}
            AnalizzaValore();
        }
        private void ComboAutista_LostFocus(object sender, RoutedEventArgs e)
        {
            AnalizzaValore();
        }
        private void AnalizzaValore()
        {
            if(ComboAutista.SelectedItem is AutistaTarga selectedAutista)
            {
                if (ComboAutista.SelectedValue != null && (int)ComboAutista.SelectedValue != -1)
                {
                    var autistaScelto = _codiciAutisti.FirstOrDefault(a => a.ID == (int)ComboAutista.SelectedValue);
                    txtTarghe.Text = autistaScelto?.Targa ?? string.Empty;
                    txtRagSocTrasp.Text = autistaScelto?.RagioneSociale ?? string.Empty;
                    autistaDaInserire = autistaScelto?.Autista ?? string.Empty;
                }                
            }
            else
            {
                string testoInserito = ComboAutista.Text.Trim().ToUpper();
                if (!string.IsNullOrEmpty(testoInserito))
                {
                    autistaDaInserire = testoInserito;
                    var autista = _codiciAutisti.FirstOrDefault(a => a.Autista.Equals(testoInserito, StringComparison.OrdinalIgnoreCase));
                    if (autista == null)
                    {
                        txtTarghe.Text = string.Empty;
                        txtRagSocTrasp.Text = string.Empty;
                    }                    
                }
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("CLIC");
            // L'utente vuole aggiornare ORA. Applica e riavvia l'app.
            _updateService.ApplicaERiavvia();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Se c'è un aggiornamento pronto ma l'utente non ha cliccato il bottone,
            // installalo silenziosamente mentre l'app si sta chiudendo.
            if (_updateService.IsUpdateReady)
            {
                _updateService.ApplicaSuChiusura();
            }
        }        
    }
}