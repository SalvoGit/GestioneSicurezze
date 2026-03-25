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

namespace GestioneSicurezze
{
    /// <summary>
    /// Logica di interazione per NuovoCliente.xaml
    /// </summary>
    public partial class NuovoCliente : Window
    {
        public NuovoCliente()
        {
            InitializeComponent();
        }

        private void SaveNewCliente_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ClienteTextBox.Text))
            {
                MessageBox.Show("Inserire un cliente valido.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                bool open = DbOperation.OpenConnection();
                if (open)
                {
                    bool clienteEsistente = DbOperation.ClienteEsistente(ClienteTextBox.Text.ToUpper());
                    if (!clienteEsistente)
                    {
                        SicurClienteCliente sicurCliente = new SicurClienteCliente
                        {
                           Cliente = ClienteTextBox.Text.ToUpper() ?? String.Empty,
                        };

                        if (DbOperation.InsertNewCliente(sicurCliente) == -1)
                        {
                            MessageBox.Show($"{DbOperation.GetErrorMessage}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            MessageBox.Show("Nuovo Cliente salvato con successo.", "Nuovo Cliente", MessageBoxButton.OK, MessageBoxImage.Information);
                            this.Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Il cliente {ClienteTextBox.Text.ToUpper()} esiste già.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch
            {
                MessageBox.Show($"Errore durante il salvataggio cliente: {DbOperation.GetErrorMessage}");
            }
            finally
            {
                DbOperation.CloseConnection();
            }
        }
    }
}
