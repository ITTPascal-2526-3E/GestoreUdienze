using BlaisePascal.GestoreUdienze.Domain.Models;
using System.Collections.Generic;
using System.Linq;

namespace BlaisePascal.GestoreUdienze.Domain.Algorithmic
{
    /// <summary>
    /// Implementazione greedy (euristica) dell'assegnazione aule.
    /// Usata come fallback rapido per scenari semplici (singolo turno, poche classi)
    /// o quando il solver CP-SAT non è disponibile/necessario.
    /// </summary>
    public class AssegnazioneAuleGreedyService : IAssegnazioneAuleService
    {
        public IEnumerable<Udienza> Assegna(
            IEnumerable<Classe> classi,
            IEnumerable<Professore> professori,
            IEnumerable<Aula> auleDisponibili,
            IEnumerable<Turno> turni)
        {
            var risultato = new List<Udienza>();
            var turniList = turni.ToList();
            
            if (!turniList.Any())
                return risultato;

            // Per il fallback greedy, assegniamo tutto al primo turno disponibile
            var turnoCorrente = turniList.First();

            // Map delle aule e di quanti professori vi sono già assegnati (massimo CapacitaMaterie)
            var auleOccupancy = auleDisponibili.ToDictionary(a => a, a => 0);

            // Per tenere traccia di quali professori hanno già un'aula
            var professoriAssegnati = new HashSet<string>();

            // Filtra solo docenti attivi
            var docentiAttivi = professori.Where(p => p.Attivo).ToList();

            // Raggruppa per classe in modo da assegnare professori della stessa classe vicini
            // Ordiniamo le classi per numero di professori decrescente (le più grandi prima)
            var classiGrouped = classi
                .GroupBy(c => c.Nome)
                .OrderByDescending(g => g.Count())
                .ToList();

            foreach (var classGroup in classiGrouped)
            {
                var professoriClasseIds = classGroup.SelectMany(c => c.DocentiIds).Distinct().ToList();
                
                // Fallback: se DocentiIds è vuoto, usa CodiceProfessore singolo
                if (!professoriClasseIds.Any())
                {
                    professoriClasseIds = classGroup.Select(c => c.CodiceProfessore)
                        .Where(cp => !string.IsNullOrEmpty(cp))
                        .Distinct()
                        .ToList();
                }

                var professoriDaAssegnare = docentiAttivi
                    .Where(p => professoriClasseIds.Contains(p.CodiceProfessore) && !professoriAssegnati.Contains(p.CodiceProfessore))
                    .ToList();

                if (!professoriDaAssegnare.Any())
                    continue;

                // Calcola la capacità disponibile per ogni piano
                var aulePerPiano = auleOccupancy
                    .Where(a => a.Value < a.Key.CapacitaMaterie)
                    .GroupBy(a => a.Key.Piano)
                    .Select(g => new
                    {
                        Piano = g.Key,
                        Aule = g.Select(x => x.Key).ToList(),
                        Capacity = g.Sum(x => x.Key.CapacitaMaterie - x.Value)
                    })
                    .OrderByDescending(p => p.Capacity)
                    .ToList();

                // Cerca un piano con abbastanza capacità o il piano con più capacità
                var pianoScelto = aulePerPiano.FirstOrDefault(p => p.Capacity >= professoriDaAssegnare.Count)
                                  ?? aulePerPiano.FirstOrDefault();

                if (pianoScelto == null)
                    break;

                var classeRef = classGroup.First();

                foreach (var prof in professoriDaAssegnare)
                {
                    // Docenti di laboratorio: assegnazione libera su qualsiasi aula
                    // Docenti normali: privilegiano piano scelto + ordinamento per ala
                    var aulaDisponibile = auleOccupancy
                        .Where(a => a.Value < a.Key.CapacitaMaterie)
                        .OrderBy(a => prof.IsLaboratorio ? 0 : (a.Key.Piano == pianoScelto.Piano ? 0 : 1))
                        .ThenBy(a => a.Key.Ala)
                        .Select(a => a.Key)
                        .FirstOrDefault();

                    if (aulaDisponibile != null)
                    {
                        risultato.Add(new Udienza
                        {
                            Docente = prof,
                            Classe = classeRef,
                            Turno = turnoCorrente,
                            Aula = aulaDisponibile
                        });

                        auleOccupancy[aulaDisponibile]++;
                        professoriAssegnati.Add(prof.CodiceProfessore);
                    }
                }
            }

            return risultato;
        }
    }
}
