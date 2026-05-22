namespace BlaisePascal.GestoreUdienze.Domain.Models
{
    public class Materia
    {
        public string CodiceMateria { get; set; } = string.Empty;
        public string NomeMateria { get; set; } = string.Empty;
        public string CodiceProfessore { get; set; } = string.Empty;

        public Materia() { }

        public Materia(string codiceMateria, string nomeMateria, string codiceProfessore)
        {
            CodiceMateria = codiceMateria;
            NomeMateria = nomeMateria;
            CodiceProfessore = codiceProfessore;
        }
    }
}
