namespace BlaisePascal.GestoreUdienze.Domain.Models
{
    public class OrarioTurni
    {
        public int Id { get; set; }
        public int Orario { get; set; }
        public string NomeProfessore { get; set; } = string.Empty;
        public string CognomeProfessore { get; set; } = string.Empty;
    }
}
