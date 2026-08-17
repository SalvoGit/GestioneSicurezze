using GestioneSicurezze.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GestioneSicurezze.AutistiTarghe
{
    /// <summary>
    /// Logica di interazione per NuovoAutista.xaml
    /// </summary>
    public partial class NuovoAutista : Window
    {
        public NuovoAutista()
        {
            InitializeComponent();
            AutistaTextBox.Focus();
        }

        private void SaveNewCliente_Click(object sender, RoutedEventArgs e)
        {
            AutistaTarga autistaTarga = new AutistaTarga
            {
                Autista = AutistaTextBox.Text.ToUpper(),
                Targa = TargaTextBox.Text.ToUpper(),
                RagioneSociale = RagioneSocialeTextBox.Text.ToUpper()
            };

            bool continua = ValidaDati(autistaTarga);

            if (continua)
            {
                try
                {
                    int targaCheck = DbOperation.VerificaTarga(autistaTarga.Targa);
                    if (targaCheck > 0)
                    {
                        MessageBoxResult messageBoxResult = MessageBox.Show("La targa inserita è già presente nel database.\n Si desidera comunque procedere con il salvataggio?", "Warning",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (messageBoxResult == MessageBoxResult.No)
                        {
                            return;
                        }
                    }
                    else if (targaCheck == -1)
                    {
                        MessageBox.Show("Errore durante il salvataggio dei dati: " + DbOperation.messageError, "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    int returnedID = DbOperation.InsertNewAutistaTarga(autistaTarga);
                    if (returnedID > 0)
                    {
                        MessageBox.Show("Dati salvati con successo!", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Errore durante il salvataggio dei dati: " + ex.Message, "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private bool ValidaDati(AutistaTarga autistaTarga)
        {
            ClearErrorMessage();
            var validator = new AutistiValidator();
            var validationResult = validator.Validate(autistaTarga);
            if (!validationResult.IsValid) {
                foreach (var error in validationResult.Errors) {
                    switch (error.PropertyName) {
                        case "Autista":
                            lblAutista.Content = error.ErrorMessage;
                            AutistaTextBox.BorderBrush = new SolidColorBrush(Colors.Red);
                            break;
                        case "Targa":
                            lblTarga.Content = error.ErrorMessage;
                            TargaTextBox.BorderBrush = new SolidColorBrush(Colors.Red);
                            break;
                        case "RagioneSociale":
                            lblRagSoc.Content = error.ErrorMessage;
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
            AutistaTextBox.BorderBrush = defaultBrush;
            TargaTextBox.BorderBrush = defaultBrush;
            RagioneSocialeTextBox.BorderBrush = defaultBrush;

            lblAutista.Content = string.Empty;
            lblTarga.Content = string.Empty;  
            lblRagSoc.Content = string.Empty;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LoadListButton_Click(object sender, RoutedEventArgs e)
        {
            CaricaListaAutisti();
        }

        private void CaricaListaAutisti()
        {
            Lis_AutistiTarghe.ItemsSource = null;
            List<AutistaTarga> autistiTarghe = DbOperation.GetAutistiTargheRagioniSociali();
            Lis_AutistiTarghe.ItemsSource = autistiTarghe;
        }
        private void AggiornaAutista(object sender, RoutedEventArgs e)
        {
            var elemento = (sender as Button).Tag as AutistaTarga;
            
            AutistaTarga autistaTarga = new AutistaTarga
            {
                ID = elemento.ID,
                Autista = elemento.Autista.ToUpper(),
                Targa = elemento.Targa.ToUpper(),
                RagioneSociale = elemento.RagioneSociale.ToUpper()
            };

            int result = DbOperation.UpadateAutistaTarga(autistaTarga);

            if(result > 0)
            {
                MessageBox.Show("Dati aggiornati con successo!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Errore durante l'aggiornamento dei dati: " + DbOperation.messageError, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void DeleteAutista(object sender, RoutedEventArgs e)
        {
            var elemento = (sender as Button).Tag as AutistaTarga;            

            int result = DbOperation.DeleteAutistaTarga(elemento.ID);

            if (result > 0)
            {
                MessageBox.Show("Dati eliminati con successo!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                CaricaListaAutisti();
            }
            else
            {
                MessageBox.Show("Errore durante l'eliminazione dei dati: " + DbOperation.messageError, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
