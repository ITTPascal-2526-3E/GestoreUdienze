namespace BlaisePascal.GestoreUdienze.Domain.Entities
{
    public class OrarioTurni
    {
        public int Id { get; set; }
        public int Orario { get; set; }
        public string NomeProfessore { get; set; } = string.Empty;
        public string CognomeProfessore { get; set; } = string.Empty;

        public OrarioTurni() { }

        public OrarioTurni(int id, int orario, string nomeProfessore, string cognomeProfessore)
        {
            Id = id;
            Orario = orario;
            NomeProfessore = nomeProfessore;
            CognomeProfessore = cognomeProfessore;
        }
    }
}
