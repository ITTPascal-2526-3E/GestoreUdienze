namespace BlaisePascal.GestoreUdienze.Domain.Entities
{
    using System;

    public class Turno
    {
        public int Id { get; set; }
        public string Giorno { get; set; } = string.Empty;
        public TimeSpan OraInizio { get; set; }
        public TimeSpan OraFine { get; set; }

        public Turno() { }

        public Turno(int id, string giorno, TimeSpan oraInizio, TimeSpan oraFine)
        {
            Id = id;
            Giorno = giorno;
            OraInizio = oraInizio;
            OraFine = oraFine;
        }
    }
}
