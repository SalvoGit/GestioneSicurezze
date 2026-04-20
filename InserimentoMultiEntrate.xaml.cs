using GestioneSicurezze.Models;
using System.Collections.ObjectModel;
using System.Windows;

using System.Windows.Input;

namespace GestioneSicurezze
{
    /// <summary>
    /// Logica di interazione per InserimentoMultiEntrate.xaml
    /// </summary>
    public partial class InserimentoMultiEntrate : Window
    {
        private ObservableCollection<MultiEntrate> multiEntrateList = new ObservableCollection<MultiEntrate>();
        public List<MultiEntrate> EntrateMultiple { get; private set; } = new List<MultiEntrate>();
        public InserimentoMultiEntrate()
        {
            InitializeComponent();
            txtEntrata.Focus();
            lis_multiEntrate.ItemsSource = multiEntrateList;
        }

        private void txtEntrata_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab || e.Key == Key.Enter) 
            {
                MultiEntrate entrata = new MultiEntrate();
                entrata.Progressivo = multiEntrateList.Count + 1;
                entrata.NrEntrata = txtEntrata.Text;
                multiEntrateList.Add(entrata);
                entrata.entrateMultiple?.Add(entrata);
                txtEntrata.Clear();
                txtEntrata.Focus();
                e.Handled = true;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            EntrateMultiple = multiEntrateList.ToList();
            this.DialogResult = true;
            this.Close();
        }
    }
}
