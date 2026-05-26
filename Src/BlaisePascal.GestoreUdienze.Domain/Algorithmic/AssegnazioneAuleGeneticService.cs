using BlaisePascal.GestoreUdienze.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlaisePascal.GestoreUdienze.Domain.Algorithmic
{
    /// <summary>
    /// Implementazione dell'assegnazione aule basata su Algoritmo Genetico.
    /// Risolve l'assegnazione dei docenti alle aule rispettando i vincoli di capacità, 
    /// floor-level grouping per le classi, e ottimizzando la prossimità e stabilità delle aule.
    /// </summary>
    public class AssegnazioneAuleGeneticService : IAssegnazioneAuleService
    {
        private class Meeting
        {
            public Professore Prof { get; }
            public Classe Classe { get; }
            public Turno Turno { get; }

            public Meeting(Professore prof, Classe classe, Turno turno)
            {
                Prof = prof;
                Classe = classe;
                Turno = turno;
            }
        }

        private class Individual
        {
            public int[] Genes { get; }
            public int Fitness { get; set; }

            public Individual(int length, Random rand, int numAule)
            {
                Genes = new int[length];
                for (int i = 0; i < length; i++)
                {
                    Genes[i] = rand.Next(numAule);
                }
            }

            public Individual(int[] genes)
            {
                Genes = (int[])genes.Clone();
            }
        }

        private readonly Random _rand = new();
        private readonly int _populationSize;
        private readonly int _generations;
        private readonly double _mutationRate;
        private readonly int _tournamentSize;

        public AssegnazioneAuleGeneticService(
            int populationSize = 100,
            int generations = 250,
            double mutationRate = 0.05,
            int tournamentSize = 5)
        {
            _populationSize = populationSize;
            _generations = generations;
            _mutationRate = mutationRate;
            _tournamentSize = tournamentSize;
        }

        public IEnumerable<Udienza> Assegna(
            IEnumerable<Classe> classi,
            IEnumerable<Professore> professori,
            IEnumerable<Aula> auleDisponibili,
            IEnumerable<Turno> turni)
        {
            var auleList = auleDisponibili.ToList();
            var profList = professori.Where(p => p.Attivo).ToList();
            var turniList = turni.ToList();

            if (!auleList.Any() || !profList.Any() || !turniList.Any())
                return Enumerable.Empty<Udienza>();

            // 1. Identifica tutti i meeting che richiedono l'assegnazione di un'aula
            var meetings = new List<Meeting>();
            foreach (var t in turniList)
            {
                foreach (var c in t.Classi)
                {
                    // Trova tutti i docenti attivi che insegnano in questa classe
                    var docentiDellaClasse = profList.Where(p =>
                        p.Classi.Any(cl => cl.Nome == c.Nome) ||
                        c.DocentiIds.Contains(p.CodiceProfessore) ||
                        c.CodiceProfessore == p.CodiceProfessore).ToList();

                    foreach (var p in docentiDellaClasse)
                    {
                        meetings.Add(new Meeting(p, c, t));
                    }
                }
            }

            if (!meetings.Any())
                return Enumerable.Empty<Udienza>();

            // 2. Esegui l'Algoritmo Genetico
            var bestChromosome = RunGeneticAlgorithm(meetings, auleList);

            // 3. Applica la soluzione migliore
            var risultato = new List<Udienza>();

            // Pulisci le assegnazioni precedenti per sicurezza
            foreach (var t in turniList)
            {
                t.AssegnazioniAule.Clear();
            }

            for (int i = 0; i < meetings.Count; i++)
            {
                var meeting = meetings[i];
                var aulaScelta = auleList[bestChromosome[i]];

                var udienza = new Udienza(meeting.Prof, meeting.Classe, meeting.Turno, aulaScelta);
                risultato.Add(udienza);

                // Assicurati che l'aula sia assegnata anche nel turno (Fase 2 Hook)
                if (!meeting.Turno.AssegnazioniAule.Any(a => a.Professore.CodiceProfessore == meeting.Prof.CodiceProfessore))
                {
                    meeting.Turno.AssegnazioniAule.Add(new AssegnazioneAula(aulaScelta, meeting.Prof));
                }
            }

            return risultato;
        }

        private int[] RunGeneticAlgorithm(List<Meeting> meetings, List<Aula> aule)
        {
            int numAule = aule.Count;
            int length = meetings.Count;

            // Inizializza la popolazione
            var population = new List<Individual>();
            for (int i = 0; i < _populationSize; i++)
            {
                var ind = new Individual(length, _rand, numAule);
                ind.Fitness = CalculateFitness(ind.Genes, meetings, aule);
                population.Add(ind);
            }

            Individual bestSoFar = population.OrderBy(ind => ind.Fitness).First();

            for (int gen = 0; gen < _generations; gen++)
            {
                // Se abbiamo già una soluzione perfetta (Fitness == 0), possiamo terminare in anticipo
                if (bestSoFar.Fitness == 0)
                    break;

                var nextGen = new List<Individual>();

                // Elitismo: mantieni i migliori
                var sorted = population.OrderBy(ind => ind.Fitness).ToList();
                nextGen.Add(new Individual(sorted[0].Genes) { Fitness = sorted[0].Fitness });
                nextGen.Add(new Individual(sorted[1].Genes) { Fitness = sorted[1].Fitness });

                while (nextGen.Count < _populationSize)
                {
                    // Selezione
                    var parent1 = TournamentSelection(population);
                    var parent2 = TournamentSelection(population);

                    // Crossover
                    int[] child1Genes, child2Genes;
                    if (_rand.NextDouble() < 0.8)
                    {
                        // Single point crossover
                        int crossoverPoint = _rand.Next(length);
                        child1Genes = new int[length];
                        child2Genes = new int[length];
                        for (int i = 0; i < length; i++)
                        {
                            if (i < crossoverPoint)
                            {
                                child1Genes[i] = parent1.Genes[i];
                                child2Genes[i] = parent2.Genes[i];
                            }
                            else
                            {
                                child1Genes[i] = parent2.Genes[i];
                                child2Genes[i] = parent1.Genes[i];
                            }
                        }
                    }
                    else
                    {
                        child1Genes = (int[])parent1.Genes.Clone();
                        child2Genes = (int[])parent2.Genes.Clone();
                    }

                    // Mutazione
                    Mutate(child1Genes, numAule);
                    Mutate(child2Genes, numAule);

                    var child1 = new Individual(child1Genes);
                    child1.Fitness = CalculateFitness(child1.Genes, meetings, aule);
                    var child2 = new Individual(child2Genes);
                    child2.Fitness = CalculateFitness(child2.Genes, meetings, aule);

                    nextGen.Add(child1);
                    if (nextGen.Count < _populationSize)
                        nextGen.Add(child2);
                }

                population = nextGen;
                var genBest = population.OrderBy(ind => ind.Fitness).First();
                if (genBest.Fitness < bestSoFar.Fitness)
                {
                    bestSoFar = new Individual(genBest.Genes) { Fitness = genBest.Fitness };
                }
            }

            return bestSoFar.Genes;
        }

        private Individual TournamentSelection(List<Individual> population)
        {
            var tournament = new List<Individual>();
            for (int i = 0; i < _tournamentSize; i++)
            {
                tournament.Add(population[_rand.Next(population.Count)]);
            }
            return tournament.OrderBy(ind => ind.Fitness).First();
        }

        private void Mutate(int[] genes, int numAule)
        {
            for (int i = 0; i < genes.Length; i++)
            {
                if (_rand.NextDouble() < _mutationRate)
                {
                    genes[i] = _rand.Next(numAule);
                }
            }
        }

        private int CalculateFitness(int[] genes, List<Meeting> meetings, List<Aula> aule)
        {
            int penalty = 0;
            int length = genes.Length;

            // Mappe temporanee per turno
            // Turno -> Aula -> List<DocenteId>
            var turniAuleOccupancy = new Dictionary<Turno, Dictionary<int, HashSet<string>>>();
            
            // Turno -> DocenteId -> HashSet<int> (aule assegnate al docente in quel turno)
            var turniDocenteAule = new Dictionary<Turno, Dictionary<string, HashSet<int>>>();

            // Turno -> Classe -> List<int> (indici aule assegnate ai docenti non lab di quella classe in quel turno)
            var turniClasseAuleNonLab = new Dictionary<Turno, Dictionary<string, List<int>>>();

            // DocenteId -> HashSet<int> (tutte le aule assegnate al docente nell'intero piano)
            var docenteTutteLeAule = new Dictionary<string, HashSet<int>>();

            // Turno -> Classe -> List<int> (tutte le aule assegnate ai docenti di quella classe, incluse lab)
            var turniClasseTutteLeAule = new Dictionary<Turno, Dictionary<string, List<int>>>();

            for (int i = 0; i < length; i++)
            {
                var meeting = meetings[i];
                int aulaIdx = genes[i];
                string docId = meeting.Prof.CodiceProfessore;
                string classeNome = meeting.Classe.Nome;

                // 1. Strutture per capacità aula per turno
                if (!turniAuleOccupancy.TryGetValue(meeting.Turno, out var auleOccupancy))
                {
                    auleOccupancy = new Dictionary<int, HashSet<string>>();
                    turniAuleOccupancy[meeting.Turno] = auleOccupancy;
                }
                if (!auleOccupancy.TryGetValue(aulaIdx, out var docentiInAula))
                {
                    docentiInAula = new HashSet<string>();
                    auleOccupancy[aulaIdx] = docentiInAula;
                }
                docentiInAula.Add(docId);

                // 2. Strutture per docente nello stesso turno
                if (!turniDocenteAule.TryGetValue(meeting.Turno, out var docenteAule))
                {
                    docenteAule = new Dictionary<string, HashSet<int>>();
                    turniDocenteAule[meeting.Turno] = docenteAule;
                }
                if (!docenteAule.TryGetValue(docId, out var auleDocente))
                {
                    auleDocente = new HashSet<int>();
                    docenteAule[docId] = auleDocente;
                }
                auleDocente.Add(aulaIdx);

                // 3. Strutture per classe e piano (non lab)
                if (!meeting.Prof.IsLaboratorio)
                {
                    if (!turniClasseAuleNonLab.TryGetValue(meeting.Turno, out var classeAuleNonLab))
                    {
                        classeAuleNonLab = new Dictionary<string, List<int>>();
                        turniClasseAuleNonLab[meeting.Turno] = classeAuleNonLab;
                    }
                    if (!classeAuleNonLab.TryGetValue(classeNome, out var auleNonLab))
                    {
                        auleNonLab = new List<int>();
                        classeAuleNonLab[classeNome] = auleNonLab;
                    }
                    auleNonLab.Add(aulaIdx);
                }

                // 4. Strutture per prossimità
                if (!turniClasseTutteLeAule.TryGetValue(meeting.Turno, out var classeTutteLeAule))
                {
                    classeTutteLeAule = new Dictionary<string, List<int>>();
                    turniClasseTutteLeAule[meeting.Turno] = classeTutteLeAule;
                }
                if (!classeTutteLeAule.TryGetValue(classeNome, out var auleClasse))
                {
                    auleClasse = new List<int>();
                    classeTutteLeAule[classeNome] = auleClasse;
                }
                auleClasse.Add(aulaIdx);

                // 5. Strutture per stabilità docente
                if (!docenteTutteLeAule.TryGetValue(docId, out var tutteAuleDoc))
                {
                    tutteAuleDoc = new HashSet<int>();
                    docenteTutteLeAule[docId] = tutteAuleDoc;
                }
                tutteAuleDoc.Add(aulaIdx);
            }

            // CALCOLO PENALITÀ VINCOLI HARD

            // V1 / V3: Docente in più aule diverse nello stesso turno -> 10000 per ogni aula in più
            foreach (var tDocAule in turniDocenteAule.Values)
            {
                foreach (var auleDoc in tDocAule.Values)
                {
                    if (auleDoc.Count > 1)
                    {
                        penalty += (auleDoc.Count - 1) * 10000;
                    }
                }
            }

            // V2 / V5: Capacità aula superata per turno -> 10000 per ogni docente in eccedenza
            foreach (var tAuleOcc in turniAuleOccupancy.Values)
            {
                foreach (var kvp in tAuleOcc)
                {
                    int aulaIdx = kvp.Key;
                    var docentiInAula = kvp.Value;
                    var aulaObj = aule[aulaIdx];
                    if (docentiInAula.Count > aulaObj.CapacitaMaterie)
                    {
                        penalty += (docentiInAula.Count - aulaObj.CapacitaMaterie) * 10000;
                    }
                }
            }

            // V4: Docenti non-lab dello stesso turno e classe su piani diversi -> 10000 per ogni piano extra
            foreach (var tClasseAuleNonLab in turniClasseAuleNonLab.Values)
            {
                foreach (var auleNonLab in tClasseAuleNonLab.Values)
                {
                    if (auleNonLab.Count > 1)
                    {
                        var piani = auleNonLab.Select(idx => aule[idx].Piano).Distinct().ToList();
                        if (piani.Count > 1)
                        {
                            penalty += (piani.Count - 1) * 10000;
                        }
                    }
                }
            }

            // CALCOLO PENALITÀ VINCOLI SOFT

            // S1: Stabilità aula docente (preferiamo la stessa aula in tutti i turni) -> 50 per ogni aula extra
            foreach (var auleDoc in docenteTutteLeAule.Values)
            {
                if (auleDoc.Count > 1)
                {
                    penalty += (auleDoc.Count - 1) * 50;
                }
            }

            // S2: Prossimità docenti della stessa classe nel turno -> Ala diversa (100) o non vicine (50)
            foreach (var tClasseTutteLeAule in turniClasseTutteLeAule.Values)
            {
                foreach (var auleClasse in tClasseTutteLeAule.Values)
                {
                    if (auleClasse.Count > 1)
                    {
                        // Controlla tutte le coppie distinte di aule della classe
                        for (int i = 0; i < auleClasse.Count; i++)
                        {
                            for (int j = i + 1; j < auleClasse.Count; j++)
                            {
                                int aIdx1 = auleClasse[i];
                                int aIdx2 = auleClasse[j];

                                if (aIdx1 == aIdx2) continue; // stessa aula, nessuna penalità di prossimità

                                var a1 = aule[aIdx1];
                                var a2 = aule[aIdx2];

                                // Se sono su piani diversi (e uno è lab, sennò violerebbe vincolo hard)
                                if (a1.Piano != a2.Piano)
                                {
                                    penalty += 200; // penalità moderata se finiscono su piani diversi
                                }

                                // Ala diversa
                                if (a1.Ala != a2.Ala)
                                {
                                    penalty += 100;
                                }

                                // Se non sono aule vicine
                                if (!a1.AuleVicine.Contains(a2.Id) && !a2.AuleVicine.Contains(a1.Id))
                                {
                                    penalty += 50;
                                }
                            }
                        }
                    }
                }
            }

            return penalty;
        }
    }
}
