using BlaisePascal.GestoreUdienze.Domain.Models;
using System.Collections.Generic;
using System.Linq;

namespace BlaisePascal.GestoreUdienze.Domain.Algorithmic
{
    public class AssegnazioneAuleService : IAssegnazioneAuleService
    {
        public IEnumerable<AssegnazioneAula> Assegna(
            IEnumerable<Classe> classi,
            IEnumerable<Professore> professori,
            IEnumerable<Aula> auleDisponibili)
        {
            var assegnazioni = new List<AssegnazioneAula>();
            
            // Map delle aule e di quanti professori vi sono già assegnati (massimo 2)
            var auleOccupancy = auleDisponibili.ToDictionary(a => a, a => 0);
            
            // Per tenere traccia di quali professori hanno già un'aula
            var professoriAssegnati = new HashSet<string>();

            // Raggruppa per classe in modo da assegnare professori della stessa classe vicini
            // Ordiniamo le classi per numero di professori decrescente (le più grandi prima)
            var classiGrouped = classi
                .GroupBy(c => c.Nome)
                .OrderByDescending(g => g.Count())
                .ToList();

            foreach (var classGroup in classiGrouped)
            {
                var professoriClasseIds = classGroup.Select(c => c.CodiceProfessore).Distinct().ToList();
                var professoriDaAssegnare = professori
                    .Where(p => professoriClasseIds.Contains(p.CodiceProfessore) && !professoriAssegnati.Contains(p.CodiceProfessore))
                    .ToList();

                if (!professoriDaAssegnare.Any())
                    continue;

                // Calcola la capacità disponibile per ogni piano
                var aulePerPiano = auleOccupancy
                    .Where(a => a.Value < 2) // Massimo 2 docenti per aula
                    .GroupBy(a => a.Key.Piano)
                    .Select(g => new 
                    {
                        Piano = g.Key,
                        Aule = g.Select(x => x.Key).ToList(),
                        Capacity = g.Count() * 2 - g.Sum(x => x.Value) // posti totali meno quelli già occupati
                    })
                    .OrderByDescending(p => p.Capacity)
                    .ToList();

                // Cerca un piano con abbastanza capacità per l'intera classe o il piano con più capacità
                var pianoScelto = aulePerPiano.FirstOrDefault(p => p.Capacity >= professoriDaAssegnare.Count) 
                                  ?? aulePerPiano.FirstOrDefault();

                if (pianoScelto == null)
                {
                    // Nessun posto disponibile nell'intero istituto
                    break;
                }

                foreach (var prof in professoriDaAssegnare)
                {
                    // Troviamo un'aula disponibile:
                    // 1. Privilegiamo il piano scelto per questa classe
                    // 2. A parità di piano, ordiniamo per Ala in modo che le aule siano vicine
                    // 3. Altrimenti fallback ad altri piani (se il piano scelto si riempie)
                    var aulaDisponibile = auleOccupancy
                        .Where(a => a.Value < 2)
                        .OrderBy(a => a.Key.Piano == pianoScelto.Piano ? 0 : 1)
                        .ThenBy(a => a.Key.Ala)
                        .Select(a => a.Key)
                        .FirstOrDefault();

                    if (aulaDisponibile != null)
                    {
                        assegnazioni.Add(new AssegnazioneAula
                        {
                            Aula = aulaDisponibile,
                            Professore = prof
                        });

                        auleOccupancy[aulaDisponibile]++;
                        professoriAssegnati.Add(prof.CodiceProfessore);
                    }
                }
            }

            return assegnazioni;
        }
    }
}
