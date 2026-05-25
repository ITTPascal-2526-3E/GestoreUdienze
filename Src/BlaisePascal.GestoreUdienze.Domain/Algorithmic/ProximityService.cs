using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlaisePascal.GestoreUdienze.Domain.Models;

namespace BlaisePascal.GestoreUdienze.Domain.Services
{
    /// <summary>
    /// Calcola la penalità di prossimità: i turni di ogni professore
    /// devono essere il più concentrati possibile nella stessa giornata.
    /// Metrica: distanza tra indice globale minimo e massimo dei turni del professore.
    /// </summary>
    public class ProximityService
    {
        /// <summary>
        /// Penalità di prossimità per un singolo professore.
        /// = (indice globale turno più tardi) - (indice globale turno più presto)
        /// </summary>
        public int PenalitaProssimita(Professore professore, PianoScheduling piano)
        {
            var indiciGlobali = piano.Turni
                .Where(t => t.Classi.Any(c => professore.Classi.Contains(c)))
                .Select(t => t.IndiceGlobale)
                .ToList();

            if (indiciGlobali.Count <= 1) return 0;

            return indiciGlobali.Max() - indiciGlobali.Min();
        }

        /// <summary>
        /// Somma delle penalità di prossimità per tutti i professori.
        /// </summary>
        public int PenalitaTotale(PianoScheduling piano, IEnumerable<Professore> professori) =>
            professori.Sum(p => PenalitaProssimita(p, piano));
    }
}

