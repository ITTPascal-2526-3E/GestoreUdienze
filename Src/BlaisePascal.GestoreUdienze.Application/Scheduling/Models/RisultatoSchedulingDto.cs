namespace BlaisePascal.GestoreUdienze.Application.Scheduling.Models
{
    using System.Collections.Generic;

    // DTO di output che rappresenta il risultato completo della schedulazione.
    // Contiene la lista di udienze assegnate e metadati sul processo di risoluzione.
    
    public class RisultatoSchedulingDto
    {
        
        // Lista di udienze assegnate dal solver.
        
        public List<UdienzaAssegnataDto> Udienze { get; set; } = new List<UdienzaAssegnataDto>();

        
        // Stato del solver: "Optimal", "Feasible", "Infeasible", "ModelInvalid", "Unknown".
        
        public string StatoSolver { get; set; } = string.Empty;

        // Tempo impiegato dal solver in secondi.
        
        public double TempoRisoluzioneSec { get; set; }

        //   Valore della funzione obiettivo (penalità totale).
        
        public double ValoreObiettivo { get; set; }

        // Eventuali warning o informazioni diagnostiche.
        
        public List<string> Warnings { get; set; } = new List<string>();

        // True se il solver ha trovato una soluzione valida (Optimal o Feasible).
        
        public bool Successo => StatoSolver == "Optimal" || StatoSolver == "Feasible";
    }

    // Rappresenta una singola udienza assegnata nel risultato della schedulazione.
    
    public class UdienzaAssegnataDto
    {
        public string CodiceProfessore { get; set; } = string.Empty;
        public string NomeProfessore { get; set; } = string.Empty;
        public int ClasseId { get; set; }
        public string ClasseNome { get; set; } = string.Empty;
        public int TurnoId { get; set; }
        public string TurnoGiorno { get; set; } = string.Empty;
        public int AulaId { get; set; }
        public string AulaNome { get; set; } = string.Empty;
        public int AulaPiano { get; set; }
    }
}
