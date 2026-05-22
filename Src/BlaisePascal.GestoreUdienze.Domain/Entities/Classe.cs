namespace BlaisePascal.GestoreUdienze.Domain.Entities
{
    using System.Collections.Generic;

    public class Classe
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Plesso { get; set; } = string.Empty;
        public int Anno { get; set; }
        public string Indirizzo { get; set; } = string.Empty;
        
        // Rimanenza per retrocompatibilità o uso corrente
        public string CodiceProfessore { get; set; } = string.Empty;
        
        // Nuova lista di docenti assegnati alla classe
        public List<string> DocentiIds { get; set; } = new List<string>();

        public Classe() { }

        public Classe(int id, string nome, string codiceProfessore)
        {
            Id = id;
            Nome = nome;
            CodiceProfessore = codiceProfessore;
        }
    }
}
