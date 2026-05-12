namespace BlaisePascal.GestoreUdienze.Domain.Models
{
    public class Aula
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string NomeProfessore1 { get; set; } = string.Empty;
        public string CognomeProfessore1 { get; set; } = string.Empty;
        public string NomeProfessore2 { get; set; } = string.Empty;
        public string CognomeProfessore2 { get; set; } = string.Empty;
    }
}
