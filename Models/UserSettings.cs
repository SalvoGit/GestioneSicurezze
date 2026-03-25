namespace GestioneSicurezze.Models
{
    public class UserSettings
    {
        public string UserPrinter { get; set; } = String.Empty;
        public int PrintCopy { get; set; } = 1;
        public string SavePath { get; set; } = String.Empty;
        public bool PrintSecur { get; set; } = false;
    }
}
