using GestioneSicurezze.Models;
using System.Windows;
using System.Windows.Controls;

namespace GestioneSicurezze.MultiAwb
{
    /// <summary>
    /// Logica di interazione per SelezionaAwbMultipli.xaml
    /// </summary>
    public partial class SelezionaAwbMultipli : Window
    {
        public List<ModelloXray>? listaAwb { get; set; }
        public ModelloXray? selectedItem { get; set; } = new();
        public SelezionaAwbMultipli()
        {
            InitializeComponent();            
        }
        public void SetAwbList(List<ModelloXray> awbList)
        {
            listaAwb = awbList;
            lis_listaAwb.ItemsSource = listaAwb;
        }
        public ModelloXray? GetSelectedItem()
        {
            return selectedItem;
        }
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Seleziona(object sender, RoutedEventArgs e)
        {
            var elemento = (sender as Button)?.Tag as ModelloXray;
            selectedItem = listaAwb?.Where(x => x.ID == elemento?.ID).FirstOrDefault();   
            this.Close();
        }
    }
}
