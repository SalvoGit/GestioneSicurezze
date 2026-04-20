using System.Collections.ObjectModel;

namespace GestioneSicurezze.Models
{
    public class MultiEntrate
    {
        public int Progressivo { get; set; }
        public string? NrEntrata { get; set; }
        public ObservableCollection<MultiEntrate>? entrateMultiple { get; set; } = new ObservableCollection<MultiEntrate>();
        public ObservableCollection<MultiEntrate> GetEntrateList()
        {
            return entrateMultiple!;
        }
    }
}
