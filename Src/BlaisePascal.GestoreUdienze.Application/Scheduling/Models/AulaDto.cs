namespace BlaisePascal.GestoreUdienze.Application.Scheduling.Models
{
    using System.Collections.Generic;

    // DTO che rappresenta un'aula nel contesto della schedulazione.
    
    public class AulaDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Ala { get; set; } = string.Empty;
        public int Piano { get; set; }
        public string Plesso { get; set; } = string.Empty;
        public int CapacitaMaterie { get; set; } = 2;
        public List<int> AuleVicine { get; set; } = new List<int>();
        public List<string> ClassiAssegnate { get; set; } = new List<string>();
    }
}
