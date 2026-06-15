using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestioneSicurezze.Models
{
    public class Sigillo
    {
        public int ID { get; set; }
        public string? NRSIGILLO { get; set; } = string.Empty;
        public DateTime DATAINSERIMENTO { get; set; } = DateTime.Now;
        public string? DESTINAZIONE { get; set; } = string.Empty;
        public string? TARGA { get; set; } = string.Empty;
        public string? TRASPORTATORE { get; set; } = string.Empty;
    }
}
