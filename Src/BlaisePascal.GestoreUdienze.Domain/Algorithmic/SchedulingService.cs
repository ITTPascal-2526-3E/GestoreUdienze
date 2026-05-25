using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlaisePascal.GestoreUdienze.Domain.Models;

namespace BlaisePascal.GestoreUdienze.Domain.Services
{
    /// <summary>
    /// Algoritmo GRASP semplificato per la distribuzione delle classi nei turni.
    ///
    /// Logica:
    /// 1. Separa le classi in Automazione (giornate 3-4) e Informatica (giornate 1-2)
    /// 2. Mescola casualmente l'ordine delle classi in ciascun gruppo
    /// 3. Assegna ogni classe al turno con penalità minima (a parità: turno meno affollato)
    /// 4. Ripete <iterations> volte e restituisce la soluzione migliore
    ///
    /// Score = (conflitti × 100) + (penalità prossimità × 1)
    /// </summary>
    public class SchedulingService
    {
        private readonly ConflictService _conflictService;
        private readonly ClassGroupingService _groupingService;
        private readonly ProximityService _proximityService;
        private readonly Random _random = new();

        public SchedulingService(
            ConflictService conflictService,
            ClassGroupingService groupingService,
            ProximityService proximityService)
        {
            _conflictService = conflictService;
            _groupingService = groupingService;
            _proximityService = proximityService;
        }

        /// <summary>
        /// Genera il miglior piano di scheduling trovato in <iterations> tentativi.
        /// </summary>
        public PianoScheduling Genera(IEnumerable<Professore> professori, int iterations = 200)
        {
            var profList = professori.ToList();

            // Estrai tutte le classi distinte e dividile per gruppo
            var tutteLeClassi = _groupingService.EstraiClassiDistinte(profList).ToList();
            var classiInformatica = _groupingService
                .FiltraPerGruppo(tutteLeClassi, GruppoSezione.Informatica).ToList();
            var classiAutomazione = _groupingService
                .FiltraPerGruppo(tutteLeClassi, GruppoSezione.Automazione).ToList();

            PianoScheduling? migliore = null;
            int migliorScore = int.MaxValue;

            for (int iter = 0; iter < iterations; iter++)
            {
                var piano = EseguiTentativo(classiInformatica, classiAutomazione, profList);
                int score = CalcolaScore(piano, profList);
                piano.Score = score;

                if (score < migliorScore)
                {
                    migliorScore = score;
                    migliore = piano;
                }

                // Soluzione perfetta trovata: fermati subito
                if (score == 0) break;
            }

            return migliore!;
        }

        private PianoScheduling EseguiTentativo(
            List<Classe> classiInformatica,
            List<Classe> classiAutomazione,
            List<Professore> professori)
        {
            var piano = PianoScheduling.Inizializza();

            // Giornate 1-2 per Informatica, giornate 3-4 per Automazione
            AssegnaGruppo(piano, Mescola(classiInformatica), GruppoSezione.Informatica, professori);
            AssegnaGruppo(piano, Mescola(classiAutomazione), GruppoSezione.Automazione, professori);

            return piano;
        }

        private void AssegnaGruppo(
            PianoScheduling piano,
            List<Classe> classi,
            GruppoSezione gruppo,
            List<Professore> professori)
        {
            var turniDisponibili = piano.GetTurniGruppo(gruppo).ToList();

            foreach (var classe in classi)
            {
                // Trova il turno con penalità minima, a parità scegli il meno affollato
                Turno? migliore = null;
                int miglioriPenalita = int.MaxValue;
                int miglioreAffollamento = int.MaxValue;

                foreach (var turno in turniDisponibili)
                {
                    if (!turno.IsDisponibile) continue;

                    int penalita = _conflictService.PenalitaTurno(turno, classe, professori);

                    if (penalita < miglioriPenalita ||
                       (penalita == miglioriPenalita && turno.NumeroClassi < miglioreAffollamento))
                    {
                        miglioriPenalita = penalita;
                        miglioreAffollamento = turno.NumeroClassi;
                        migliore = turno;
                    }
                }

                migliore?.AggiungiClasse(classe);
            }
        }

        private int CalcolaScore(PianoScheduling piano, List<Professore> professori)
        {
            int conflitti = _conflictService.ScoreConflittiTotale(piano, professori);
            int prossimita = _proximityService.PenalitaTotale(piano, professori);
            return (conflitti * 100) + prossimita;
        }

        private List<Classe> Mescola(List<Classe> classi)
        {
            var copia = classi.ToList();
            for (int i = copia.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (copia[i], copia[j]) = (copia[j], copia[i]);
            }
            return copia;
        }
    }
}

