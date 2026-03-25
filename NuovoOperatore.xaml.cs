using GestioneSicurezze.Models;
using System.Windows;

namespace GestioneSicurezze
{
    /// <summary>
    /// Logica di interazione per NuovoOperatore.xaml
    /// </summary>
    public partial class NuovoOperatore : Window
    {
        public NuovoOperatore()
        {
            InitializeComponent();
            NewOpCodtxt.Focus();
        }

        private void SaveNewOperator_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewOpCodtxt.Text))
            {
                MessageBox.Show("Inserisci un codice operatore valido.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(NewOpCodtxt.Text, out int codiceOperatore))
            {
                MessageBox.Show("Codice operatore può essere solo un valore numerico.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(NewSedetxt.Text))
            {
                MessageBox.Show("Inserire una sede valida.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                bool open = DbOperation.OpenConnection();
                if (open) 
                {
                    bool operatoreEsistente = DbOperation.OperatoreEsistente(codiceOperatore);
                    if (!operatoreEsistente)
                    {
                        CodiciOperatori codiciOperatori = new CodiciOperatori { Codice_Operatore = codiceOperatore,
                            Nome_Cognome = NewOpNomCogntxt.Text.ToUpper() ?? String.Empty,
                            Sede = NewSedetxt.Text.ToUpper() ?? String.Empty,
                        };

                        if (DbOperation.InsertNewOperator(codiciOperatori) == -1)
                        {
                            MessageBox.Show($"{DbOperation.GetErrorMessage}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);                        
                        }
                        else
                        {
                            MessageBox.Show("Nuovo Operatore salvato con successo.", "Nuovo Operatore", MessageBoxButton.OK, MessageBoxImage.Information);
                            this.Close();
                        }
                    }
                    else
                    {
                            MessageBox.Show($"Il codice operatore {codiceOperatore} esiste già.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch
            {
                MessageBox.Show($"Errore durante il salvataggio Nuovo Operatore: {DbOperation.GetErrorMessage}");
            }
            finally
            {
                DbOperation.CloseConnection();
            }
        }
    }    
}
