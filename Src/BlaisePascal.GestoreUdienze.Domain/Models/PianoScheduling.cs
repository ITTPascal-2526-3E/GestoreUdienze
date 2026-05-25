using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlaisePascal.GestoreUdienze.Domain.Models
{
    /// <summary>
    /// Aggregato radice del piano di scheduling.
    /// Contiene tutti i 36 turni (4 giornate × 9 turni).
    /// Score: meno è meglio. 0 = nessun conflitto.
    /// </summary>
    public class PianoScheduling
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime GeneratoIl { get; set; } = DateTime.Now;

        /// <summary>
        /// score = (conflitti × 100) + (penalità prossimità × 1)
        /// </summary>
        public int Score { get; set; }

        public List<Turno> Turni { get; set; } = new();

        public PianoScheduling() { }

        /// <summary>
        /// Inizializza i 36 turni vuoti.
        /// </summary>
        public static PianoScheduling Inizializza()
        {
            var piano = new PianoScheduling();
            for (int giornata = 1; giornata <= 4; giornata++)
                for (int idx = 1; idx <= 4; idx++)
                    piano.Turni.Add(new Turno(giornata, idx));
            return piano;
        }

        public Turno GetTurno(int numeroGiornata, int indiceTurno) =>
            Turni.First(t => t.NumeroGiornata == numeroGiornata && t.IndiceTurno == indiceTurno);

        public IEnumerable<Turno> GetTurniGiornata(int numeroGiornata) =>
            Turni.Where(t => t.NumeroGiornata == numeroGiornata);

        public IEnumerable<Turno> GetTurniGruppo(GruppoSezione gruppo) =>
            Turni.Where(t => t.Gruppo == gruppo);

        /// <summary>
        /// Hook Fase 2: professori coinvolti in un turno.
        /// </summary>
        public IEnumerable<Professore> GetProfessoriPerTurno(Turno turno, IEnumerable<Professore> tutti) =>
            tutti.Where(p => p.Classi.Any(c => turno.Classi.Contains(c)));

        public override string ToString() =>
            $"PianoScheduling [{Id}] Score: {Score} - {GeneratoIl:dd/MM/yyyy HH:mm}";
    }
}

