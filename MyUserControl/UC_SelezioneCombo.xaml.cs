using System.Windows.Controls;

namespace GestioneSicurezze.MyUserControl
{
    /// <summary>
    /// Logica di interazione per UC_SelezioneCombo.xaml
    /// </summary>
    public partial class UC_SelezioneCombo : UserControl
    {
        public UC_SelezioneCombo()
        {
            InitializeComponent();
        }

        public ComboBox ComboEnacBox => ComboEnac;
        public ComboBox ComboOperatoriBox => ComboOperatori;
        public ComboBox ComboClienteBox => ComboCliente;
        public Label ErrCodEU => lblCodEU;
        public Label ErrOperatore => lblOperatore;
        public Label ErrCliente => lblCliente;

    }
}
