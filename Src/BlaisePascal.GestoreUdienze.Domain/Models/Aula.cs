namespace BlaisePascal.GestoreUdienze.Domain.Models
{
    using System.Collections.Generic;

    public class Aula
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Ala { get; set; } = string.Empty;
        public int Piano { get; set; }
        
        public string Plesso { get; set; } = string.Empty;
        public int CapacitaMaterie { get; set; } = 2;
        public List<int> AuleVicine { get; set; } = new List<int>();
        public List<string> ClassiAssegnate { get; set; } = new List<string>();

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
