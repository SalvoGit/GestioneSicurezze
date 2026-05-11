namespace GestioneSicurezze.Models
{
    public class UserSettings
    {
        public string UserPrinter { get; set; } = String.Empty;
        public int PrintCopy { get; set; } = 1;
        public string SavePath { get; set; } = String.Empty;
        public bool PrintSecur { get; set; } = false;
        public string DefaultEnacCode { get; set; } = String.Empty;
        public int DefaultOperator { get; set; } = 1;
        public string DefaultClient { get; set; } = String.Empty;
    }
}
