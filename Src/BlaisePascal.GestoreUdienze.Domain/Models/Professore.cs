namespace BlaisePascal.GestoreUdienze.Domain.Models
{
    public class Professore
    {
        public string CodiceProfessore { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Cognome { get; set; } = string.Empty;

        public Professore() { }
        
        public Professore(string codiceProfessore, string nome, string cognome)
        {
            CodiceProfessore = codiceProfessore;
            Nome = nome;
            Cognome = cognome;
        }
    }
}
