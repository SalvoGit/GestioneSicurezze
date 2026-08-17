using System.Windows;
using Velopack;

namespace GestioneSicurezze.VelopackService
{
    public class VelopackUpdateService
    {
        // UpdateManager è la classe principale per gestire gli aggiornamenti di Velopack
        private UpdateManager _updateManager;

        // UpdateInfo rappresenta le informazioni sull'aggiornamento disponibile sempre da Velopack
        private UpdateInfo _pendingUpdate;

        // Espone lo stato: true se l'aggiornamento è scaricato e pronto per l'installazione
        /// <summary>
        /// Indica se un aggiornamento è pronto per essere applicato. Viene impostato su true dopo che l'aggiornamento è stato scaricato con successo.
        /// </summary>
        public bool IsUpdateReady { get; private set; } = false;

        //Costruttore che accetta l'URL dell'aggiornamento come parametro
        public VelopackUpdateService(string updateUrl)
        {
            _updateManager = new UpdateManager(updateUrl);
        }

        /// <summary>
        /// Controlla se ci sono aggiornamenti e, se presenti, li scarica in background.
        /// </summary>
        public async Task<bool> PrepareUpdateAsync()
        {
            // Se l'app non è installata (es. stai facendo debug da Visual Studio), ignora
            if (!_updateManager.IsInstalled) return false;

            try
            {
                // 1. Controlla la presenza di una nuova versione
                _pendingUpdate = await _updateManager.CheckForUpdatesAsync();

                if (_pendingUpdate == null) return false;

                // 2. Scarica i file necessari in background
                await _updateManager.DownloadUpdatesAsync(_pendingUpdate);

                IsUpdateReady = true;
                return true;
            }
            catch (Exception ex)
            {
                // Qui puoi loggare eventuali errori di rete o permessi
                MessageBox.Show($"Errore durante il controllo aggiornamenti: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Applica l'aggiornamento scaricato e riavvia subito l'applicazione.
        /// </summary>
        public void ApplicaERiavvia()
        {
            if (IsUpdateReady && _pendingUpdate != null)
            {
                _updateManager.ApplyUpdatesAndRestart(_pendingUpdate);
            }
        }

        /// <summary>
        /// Applica l'aggiornamento chiudendo l'app, senza riavviarla (utile quando l'utente esce).
        /// </summary>
        public void ApplicaSuChiusura()
        {
            if (IsUpdateReady && _pendingUpdate != null)
            {
                _updateManager.ApplyUpdatesAndExit(_pendingUpdate);
            }
        }
        /// <summary>
        /// Restituisce la versione corrente dell'applicazione.
        /// </summary>
        public string GetCurrentVersion()
        {
            var v = _updateManager.CurrentVersion;            
            return v?.ToFullString() ?? string.Empty;
        }
    }
}