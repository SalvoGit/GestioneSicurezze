using GestioneSicurezze.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestioneSicurezze
{
    public class RecordNonTrovatoException : Exception
    {
        public RecordNonTrovatoException(string message) : base(message)
        {
        }
    }
    public static class DeleteUtility
    {
        private static string message = string.Empty;
        public static void DeletePDFSicurezza(int progressivo, string cliente, string awb, string savePath)
        {
            string fileName = $"{progressivo}_{cliente.Replace(" ", "_")}_{awb}.pdf";
            string filePath = Path.Combine(savePath, fileName);
            
            if(File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public static void DeleteSicurezzaFromDatabase(int id, string sicurezza)
        {
            message = string.Empty;
            var righeCancellate = DbOperation.DeleteSicurezza(id, sicurezza);

            if(righeCancellate == -1)
            {
                throw new Exception($"Errore durante la cancellazione della sicurezza dal database.\n Errore: {DbOperation.GetErrorMessage}");
                //message = $"Errore durante la cancellazione della sicurezza dal database.\n Errore: {DbOperation.GetErrorMessage}";
                //return false;
            }

            if(righeCancellate == 0)
            {
                throw new RecordNonTrovatoException($"Nessuna riga trovata per la sicurezza con ID {id} e AWB {sicurezza}.");
                //message = $"Nessuna riga trovata per la sicurezza con ID {id} e AWB {sicurezza}.";
                //return false;
            }

            if(righeCancellate > 1)
            {
                throw new Exception($"Trovate {righeCancellate} righe per la sicurezza con ID {id} e AWB {sicurezza}.");
                //return false;
            }

            message = $"Sicurezza AWB {sicurezza} cancellata con successo.";
            //return true;
        }

        public static string GetMessage()
        {
            // Log the error message or display it to the user
            return message;
        }
    }
}
