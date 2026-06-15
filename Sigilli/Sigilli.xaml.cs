using GestioneSicurezze.Models;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace GestioneSicurezze
{
    /// <summary>
    /// Logica di interazione per Sigilli.xaml
    /// </summary>
    public partial class Sigilli : Window
    {
        public Sigilli()
        {
            InitializeComponent();
            CaricaUltimiSigilli();
            txtNrSigillo.Focus();
        }

        private void CaricaUltimiSigilli()
        {
            List<Sigillo> sigilli = DbOperation.GetAllSigilli();
            
            if(sigilli is null)
            {
                MessageBox.Show($"Errore nel recupero degli ultimi sigilli inseriti.\n\nContattare il servizio IT.\n\nErrore: {DbOperation.GetErrorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            lis_UltimiInseriti.ItemsSource = sigilli;

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void InsertButton_Click(object sender, RoutedEventArgs e)
        {
            ClearErrorMessage();
            bool isValid = VerificaInput();
            
            if(isValid)
            {
                Sigillo sigillo = new Sigillo
                {
                    NRSIGILLO = txtNrSigillo.Text.ToUpper(),
                    DESTINAZIONE = txtDestinazione.Text.ToUpper(),
                    TARGA = txtTarga.Text.ToUpper(),
                    TRASPORTATORE = txtSocieta.Text.ToUpper()
                };

                int result = DbOperation.SalvaSigillo(sigillo);

                if(result == -1)
                {
                    MessageBox.Show("Sigillo salvato correttamente!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if(result == -2)
                {
                        MessageBox.Show($"Errore durante il salvataggio del sigillo!\nErrore : {DbOperation.messageError}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }

                CaricaUltimiSigilli();
            }
        }        
        private bool VerificaInput()
        {
            ValidazioneSigilli validazione = new ValidazioneSigilli();
            var risultato = validazione.Validate(new Sigillo
            {
                NRSIGILLO = txtNrSigillo.Text,
                DESTINAZIONE = txtDestinazione.Text,
                TARGA = txtTarga.Text,
                TRASPORTATORE = txtSocieta.Text
            });

            if (!risultato.IsValid)
            {
                foreach (var r in risultato.Errors)
                {
                    switch (r.PropertyName)
                    {
                        case nameof(Sigillo.NRSIGILLO):
                            txtNrSigillo.BorderBrush = Brushes.Red;
                            lblSigillo.Content = r.ErrorMessage;
                            break;
                        case nameof(Sigillo.DESTINAZIONE):
                            txtDestinazione.BorderBrush = Brushes.Red;
                            lblDestinazione.Content = r.ErrorMessage;
                            break;
                        case nameof(Sigillo.TARGA):
                            txtTarga.BorderBrush = Brushes.Red;
                            lblTarga.Content = r.ErrorMessage;
                            break;
                        case nameof(Sigillo.TRASPORTATORE):
                            txtSocieta.BorderBrush = Brushes.Red;
                            lblSocieta.Content = r.ErrorMessage;
                            break;
                    }
                }

                MessageBox.Show("Prima di poter effettuare il salvataggio è necessario correggere gli errori evidenziati", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Stop);
                return false;
            }

            return true;
        }

        private void ClearErrorMessage()
        {
            SolidColorBrush defaultBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFABADB3"));
            txtNrSigillo.BorderBrush = defaultBrush;
            txtDestinazione.BorderBrush = defaultBrush;
            txtSocieta.BorderBrush = defaultBrush;
            txtTarga.BorderBrush = defaultBrush;

            lblSigillo.Content = string.Empty;
            lblDestinazione.Content = string.Empty;
            lblSocieta.Content = string.Empty;
            lblTarga.Content = string.Empty;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRicerca.Text))
            {
                MessageBox.Show("Inserire un termine valido per la ricerca", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string criterio = string.Empty;           

            switch (cmbCriteri.SelectedValue.ToString().ToUpper())
            {
                case "SIGILLO":
                    criterio = "NRSIGILLO";
                    break;
                case "DESTINAZIONE":
                    criterio = "DESTINAZIONE";
                    break;
                case "TARGA":
                    criterio = "TARGA";
                    break;
                case "TRASPORTATORE":
                    criterio = "TRASPORTATORE";
                    break;
                default:
                    criterio = "NRSIGILLO";
                    break;
            }

            List<Sigillo> sigilli = DbOperation.SearchByString(txtRicerca.Text, criterio);
            lis_UltimiInseriti.ItemsSource = sigilli;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            CaricaUltimiSigilli();
            txtRicerca.Clear();
            txtNrSigillo.Focus();
        }
    }
}