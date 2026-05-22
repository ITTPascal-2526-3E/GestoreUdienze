namespace BlaisePascal.GestoreUdienze.Domain.Entities
{
    using System.Collections.Generic;

    public class Professore
    {
        public string CodiceProfessore { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Cognome { get; set; } = string.Empty;
        public bool Attivo { get; set; } = true;
        public bool ELaboratorio { get; set; } = false;
        public string Plesso { get; set; } = string.Empty;
        
        public List<string> ClassiInsegnate { get; set; } = new List<string>();
        public Dictionary<string, List<string>> MateriePerClasse { get; set; } = new Dictionary<string, List<string>>();

        public Professore() { }
        
        public Professore(string codiceProfessore, string nome, string cognome)
        {
            CodiceProfessore = codiceProfessore;
            Nome = nome;
            Cognome = cognome;
        }
    }
}
