using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlaisePascal.GestoreUdienze.Domain.Models;

namespace BlaisePascal.GestoreUdienze.Domain.Services
{
    /// <summary>
    /// Divide le classi nei due gruppi: Automazione (A–D) e Informatica (E–N, BIO).
    /// </summary>
    public class ClassGroupingService
    {
        public GruppoSezione GetGruppo(Classe classe) => classe.Gruppo;

        public IEnumerable<Classe> FiltraPerGruppo(
            IEnumerable<Classe> classi,
            GruppoSezione gruppo) =>
            classi.Where(c => c.Gruppo == gruppo);

        /// <summary>
        /// Estrae tutte le classi distinte dall'insieme dei professori.
        /// </summary>
        public IEnumerable<Classe> EstraiClassiDistinte(IEnumerable<Professore> professori) =>
            professori
                .SelectMany(p => p.Classi)
                .DistinctBy(c => c.Nome)
                .OrderBy(c => c.Anno)
                .ThenBy(c => c.Sezione);
    }
}

