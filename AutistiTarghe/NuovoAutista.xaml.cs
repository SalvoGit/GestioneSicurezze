using GestioneSicurezze.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
    }
}
