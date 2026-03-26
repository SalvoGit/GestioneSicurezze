namespace GestioneSicurezze.Models
{
    public class ModelloXray
    {
        public int ID { get; set; }
        public int Progressivo { get; set; }
        public string NrEntrata { get; set; } = String.Empty;
        public string? CodiceEnac { get; set; }
        public string? Awb { get; set; }
        public string? Colli { get; set; }
        public string? Peso { get; set; }
        public string? Destinazione { get; set; }
        public string? Contenuto { get; set; }
        public string? Cliente { get; set; }
        public string? Operatore { get; set; }
        public DateTime DataEsecuzione { get; set; } = DateTime.Now;
        public string Riferimento { get; set; } = String.Empty;
        public string Trasportatore { get; set; } = String.Empty;
        public string Targa { get; set; } = String.Empty;
        public string SigilloNumero { get; set; } = String.Empty;
        public string Autista { get; set; } = String.Empty;
        public bool XRAY { get; set; } = true;
        public int QT_XRAY { get; set; } = 0;
        public bool ETD { get; set; } = false;
        public int QT_ETD { get; set; } = 0;
        public bool PHS { get; set; } = false;
        public int QT_PHS { get; set; } = 0;
        public bool VCK { get; set; } = false;
        public int QT_VCK { get; set; } = 0;
        public string? STATOMERCE { get; set; } = "SPX";        
        public string? AeroportoDest { get; set; } = String.Empty;
    }
}
