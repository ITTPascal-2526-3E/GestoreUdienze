namespace BlaisePascal.GestoreUdienze.Application.Scheduling.Models
{
    using System.Collections.Generic;

    // DTO che rappresenta una classe scolastica nel contesto della schedulazione.
    
    public class ClasseDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Plesso { get; set; } = string.Empty;
        public int Anno { get; set; }
        public string Indirizzo { get; set; } = string.Empty;
        public List<string> DocentiIds { get; set; } = new List<string>();
    }
}
