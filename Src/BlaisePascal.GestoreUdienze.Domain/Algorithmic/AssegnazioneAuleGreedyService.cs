using BlaisePascal.GestoreUdienze.Domain.Models;
using System.Collections.Generic;

namespace BlaisePascal.GestoreUdienze.Domain.Algorithmic
{
    /// <summary>
    /// Servizio delegato per l'assegnazione aule.
    /// Utilizza internamente l'algoritmo genetico ottimizzato per trovare la migliore disposizione dei professori nelle aule.
    /// </summary>
    public class AssegnazioneAuleGreedyService : IAssegnazioneAuleService
    {
        private readonly AssegnazioneAuleGeneticService _geneticService;

        public AssegnazioneAuleGreedyService()
        {
            // Parametri GA di default ottimizzati per convergenza e velocità
            _geneticService = new AssegnazioneAuleGeneticService(
                populationSize: 120,
                generations: 300,
                mutationRate: 0.08,
                tournamentSize: 5
            );
        }

        public IEnumerable<Udienza> Assegna(
            IEnumerable<Classe> classi,
            IEnumerable<Professore> professori,
            IEnumerable<Aula> auleDisponibili,
            IEnumerable<Turno> turni)
        {
            return _geneticService.Assegna(classi, professori, auleDisponibili, turni);
        }
    }
}
