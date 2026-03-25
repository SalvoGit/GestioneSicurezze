using GestioneSicurezze.Models;
using Ookii.Dialogs.Wpf;
using System.Windows;

namespace GestioneSicurezze
{
    /// <summary>
    /// Logica di interazione per Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
        }

        public void LoadPrinter(object sender, RoutedEventArgs e)
        {         
            PrinterComboBox.ItemsSource = UtilitySettings.LoadPrinter().Select(p => p.Name);
            LoadActualSettings();
        }

        private void LoadActualSettings()
        {
            UserSettings userSettings = UtilitySettings.ReadActualSettings();
            PrinterComboBox.SelectedItem = userSettings.UserPrinter;
            NumeroCopieTextBox.Text = userSettings.PrintCopy.ToString();
            txtSavePath.Text = userSettings.SavePath;
            if(userSettings.PrintSecur)
            {
                rbPrintYes.IsChecked = true;
            }
            else
            {
                rbPrintNo.IsChecked = true;
            }
        }

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            int numeroCopie;               

            if (PrinterComboBox.SelectedItem == null)
            {
                MessageBox.Show("Selezionare una stampante tra quelle disponobili.", "Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if(String.IsNullOrWhiteSpace(NumeroCopieTextBox.Text) || !int.TryParse(NumeroCopieTextBox.Text, out numeroCopie) || numeroCopie <= 0)
            {
                MessageBox.Show("Inserire un numero valido di copie da stampare.", "Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if(String.IsNullOrWhiteSpace(txtSavePath.Text))
            {
                MessageBox.Show("Inserire un percorso valido per salvare i file PDF.", "Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool printSecur = rbPrintYes.IsChecked == true;

            string messaggio = UtilitySettings.SaveSettings(PrinterComboBox.SelectedItem as string ?? string.Empty, numeroCopie, txtSavePath.Text, printSecur);            

            MessageBox.Show(messaggio, "Settings", MessageBoxButton.OK, MessageBoxImage.Information);            
        }
        private void SaveSettingsClose(object sender, RoutedEventArgs e)
        {
            SaveSettings(sender, e);
            this.Close();
        }


        private void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog {
                Description = "Seleziona la cartella di destinazione per i file PDF",
                UseDescriptionForTitle = true
            };

            bool? result = dialog.ShowDialog();
            if (result == true) { 
                txtSavePath.Text = dialog.SelectedPath; 
            }
        }
    }
}
