using System.Configuration;
using System.Data;
using System.Windows;
using Velopack;

namespace GestioneSicurezze
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [STAThread]
        private static void Main(string[] args)
        {
            // Esegui il setup di Velopack il prima possibile.
            // Se Velopack rileva che deve eseguire azioni di installazione,
            // lo farà per poi terminare automaticamente il processo.
            VelopackApp.Build().Run();

            var app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }

}
