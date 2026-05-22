namespace BlaisePascal.GestoreUdienze.Application.Scheduling.Models
{
    using System.Collections.Generic;

    // DTO che rappresenta un docente nel contesto della schedulazione.
    // 
    public class DocenteDto
    {
        public string CodiceProfessore { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Cognome { get; set; } = string.Empty;
        public bool Attivo { get; set; } = true;
        public bool IsLaboratorio { get; set; } = false;
        public string Plesso { get; set; } = string.Empty;
        public List<string> ClassiInsegnate { get; set; } = new List<string>();
        public Dictionary<string, List<string>> MateriePerClasse { get; set; } = new Dictionary<string, List<string>>();
    }
}
