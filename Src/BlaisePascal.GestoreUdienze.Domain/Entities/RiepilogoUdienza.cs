namespace BlaisePascal.GestoreUdienze.Domain.Entities
{
    public class RiepilogoUdienza
    {
        public string Nominativo { get; set; } = string.Empty;
        public string Classi { get; set; } = string.Empty;
        public string Materie { get; set; } = string.Empty;
        public string Aula { get; set; } = string.Empty;
        public string Piano { get; set; } = string.Empty;
        public string Turno { get; set; } = string.Empty;
        public string Orario { get; set; } = string.Empty;

        public RiepilogoUdienza() { }

        public RiepilogoUdienza(string nominativo, string classi, string materie, string aula, string piano, string turno, string orario)
        {
            Nominativo = nominativo;
            Classi = classi;
            Materie = materie;
            Aula = aula;
            Piano = piano;
            Turno = turno;
            Orario = orario;
        }
    }
}
