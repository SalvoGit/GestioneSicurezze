using GestioneSicurezze.Models;
using System.IO;
using System.Printing;
using System.Text.Json;

namespace GestioneSicurezze
{
    public static class UtilitySettings
    {
        private readonly static string directorySettings = "GestioneSicurezze";
        private readonly static string fileSettings = "usersettings.json";
        private readonly static string saveDirectory = @"C:\Sviluppo\Sicurezze";
        private readonly static bool defaultPrintSecur = true;
        public static PrintQueueCollection LoadPrinter()
        {
            var printServer = new LocalPrintServer();
            var printers = printServer.GetPrintQueues(new[] { EnumeratedPrintQueueTypes.Local, EnumeratedPrintQueueTypes.Connections });
            return printers;            
        }
        public static IReadOnlyList<CodiciEnac> GetEnacCodes()
        {
            return DbOperation.CodiciEnac();
        }
        public static IReadOnlyList<CodiciOperatori> GetOperatorCodes()
        {
            return DbOperation.CodiciOperatori();
        }
        public static IReadOnlyList<SicurClienteCliente> GetClienteCodes()
        {
            return DbOperation.NominativiClienti();
        }
        public static UserSettings ReadActualSettings()
        {
            string percorso = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), directorySettings, fileSettings);

            if (File.Exists(percorso))
            {
                string jsonString = File.ReadAllText(percorso);
                var userSettings = JsonSerializer.Deserialize<UserSettings>(jsonString);
                if (userSettings != null)
                {
                    return userSettings;                    
                }                
            }

            return new UserSettings { PrintCopy = 1, UserPrinter = String.Empty, SavePath = saveDirectory, PrintSecur = defaultPrintSecur, DefaultEnacCode = String.Empty }; 
        }

        public static string SaveSettings(string Printer, int Copies, string savePath, bool printSecur, string defaultEnac, int defaultOperatore, string defaultClient)
        {
            try
            {
                var userSettings = new UserSettings
                {
                    UserPrinter = Printer,
                    PrintCopy = Copies,
                    SavePath = savePath,
                    PrintSecur = printSecur,
                    DefaultEnacCode = defaultEnac,
                    DefaultOperator = defaultOperatore,
                    DefaultClient = defaultClient
                };
                string jsonString = JsonSerializer.Serialize(userSettings, new JsonSerializerOptions { WriteIndented = true });
                string percorso = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), directorySettings, fileSettings);
                Directory.CreateDirectory(Path.GetDirectoryName(percorso));
                File.WriteAllText(percorso, jsonString);
                return "Impostazioni salvate correttamente";
            }
            catch (Exception ex)
            {
                return $"Errore durante il salvataggio delle impostazioni:{ex.Message})";
            }            
        }
    }
}
