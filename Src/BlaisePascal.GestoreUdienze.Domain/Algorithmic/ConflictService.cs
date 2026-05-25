using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlaisePascal.GestoreUdienze.Domain.Models;

namespace BlaisePascal.GestoreUdienze.Domain.Services
{
    /// <summary>
    /// Calcola conflitti tra classi in base ai professori condivisi.
    /// Due classi sono in conflitto se almeno un professore non esente insegna in entrambe.
    /// </summary>
    public class ConflictService
    {
        /// <summary>
        /// True se le due classi condividono almeno un professore non esente.
        /// </summary>
        public bool HaConflitto(Classe a, Classe b, IEnumerable<Professore> professori)
        {
            foreach (var prof in professori)
            {
                if (prof.IsEsente) continue;
                if (prof.Classi.Contains(a) && prof.Classi.Contains(b))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Penalità = numero di classi già nel turno che conflittano con la candidata.
        /// </summary>
        public int PenalitaTurno(Turno turno, Classe candidata, IEnumerable<Professore> professori)
        {
            int penalita = 0;
            foreach (var esistente in turno.Classi)
                if (HaConflitto(esistente, candidata, professori))
                    penalita++;
            return penalita;
        }

        /// <summary>
        /// Score conflitti totale del piano: ogni coppia in conflitto nello stesso turno = +1.
        /// </summary>
        public int ScoreConflittiTotale(PianoScheduling piano, IEnumerable<Professore> professori)
        {
            int score = 0;
            var profList = professori.ToList();

            foreach (var turno in piano.Turni)
            {
                var classi = turno.Classi.ToList();
                for (int i = 0; i < classi.Count; i++)
                    for (int j = i + 1; j < classi.Count; j++)
                        if (HaConflitto(classi[i], classi[j], profList))
                            score++;
            }
            return score;
        }
    }
}

