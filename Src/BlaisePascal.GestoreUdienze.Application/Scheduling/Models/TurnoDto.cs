namespace BlaisePascal.GestoreUdienze.Application.Scheduling.Models
{
    using System;

    // DTO che rappresenta un turno temporale nel contesto della schedulazione.
    
    public class TurnoDto
    {
        public int Id { get; set; }
        public string Giorno { get; set; } = string.Empty;
        public TimeSpan OraInizio { get; set; }
        public TimeSpan OraFine { get; set; }
    }
}
