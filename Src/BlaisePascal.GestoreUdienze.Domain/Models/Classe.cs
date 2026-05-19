namespace BlaisePascal.GestoreUdienze.Domain.Models
{
    public class Classe
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string CodiceProfessore { get; set; } = string.Empty;

        public Classe() { }

        public Classe(int id, string nome, string codiceProfessore)
        {
            Id = id;
            Nome = nome;
            CodiceProfessore = codiceProfessore;
        }
    }
}
