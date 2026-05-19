namespace BlaisePascal.GestoreUdienze.Domain.Models
{
    public class Aula
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Ala { get; set; } = string.Empty;
        public int Piano { get; set; }

        public Aula() { }

        public Aula(int id, string nome, string ala, int piano)
        {
            Id = id;
            Nome = nome;
            Ala = ala;
            Piano = piano;
        }
    }
}
