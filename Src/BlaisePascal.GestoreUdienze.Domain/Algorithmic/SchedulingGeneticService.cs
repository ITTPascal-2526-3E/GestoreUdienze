using System;
using System.Collections.Generic;
using System.Linq;
using BlaisePascal.GestoreUdienze.Domain.Models;
using BlaisePascal.GestoreUdienze.Domain.Services;

namespace BlaisePascal.GestoreUdienze.Domain.Algorithmic
{
    public class SchedulingGeneticService(
        ConflictService conflictService,
        ClassGroupingService groupingService,
        ProximityService proximityService,
        int populationSize = 150,
        int generations = 300,
        double mutationRate = 0.06,
        int tournamentSize = 5)
    {
        private readonly Random _rand = new();

        private class Individual
        {
            public int[] Genes { get; }
            public int Fitness { get; set; }

            public Individual(int length)
            {
                Genes = new int[length];
            }

            public Individual(int[] genes)
            {
                Genes = (int[])genes.Clone();
            }
        }

        public PianoScheduling Genera(IEnumerable<Professore> professori)
        {
            var profList = professori.ToList();
            var tutteLeClassi = groupingService.EstraiClassiDistinte(profList).ToList();
            
            var classiInformatica = groupingService.FiltraPerGruppo(tutteLeClassi, GruppoSezione.Informatica).ToList();
            var classiAutomazione = groupingService.FiltraPerGruppo(tutteLeClassi, GruppoSezione.Automazione).ToList();

            var pianoInformatica = RunGeneticAlgorithm(classiInformatica, profList, GruppoSezione.Informatica);
            var pianoAutomazione = RunGeneticAlgorithm(classiAutomazione, profList, GruppoSezione.Automazione);

            var piano = PianoScheduling.Inizializza();
            MergePiano(piano, pianoInformatica);
            MergePiano(piano, pianoAutomazione);
            
            piano.Score = CalcolaScore(piano, profList);
            return piano;
        }

        private static void MergePiano(PianoScheduling piano, PianoScheduling subPiano)
        {
            foreach(var t in subPiano.Turni)
            {
                if(t.Classi.Count > 0)
                {
                    var dest = piano.GetTurno(t.NumeroGiornata, t.IndiceTurno);
                    foreach(var c in t.Classi)
                    {
                        dest.AggiungiClasse(c);
                    }
                }
            }
        }

        private PianoScheduling RunGeneticAlgorithm(List<Classe> classi, List<Professore> professori, GruppoSezione gruppo)
        {
            if(classi.Count == 0) return PianoScheduling.Inizializza();
            
            int length = classi.Count;
            int[] numGiornate = gruppo == GruppoSezione.Informatica ? [1, 2] : [3, 4];
            
            var validTurnIndices = new List<(int giornata, int turno)>();
            foreach(var g in numGiornate)
            {
                for(int t=1; t<=4; t++) validTurnIndices.Add((g, t));
            }
            int numTurni = validTurnIndices.Count;

            var population = new List<Individual>();
            for (int i = 0; i < populationSize; i++)
            {
                var ind = new Individual(length);
                for(int g=0; g<length; g++) ind.Genes[g] = _rand.Next(numTurni);
                ind.Fitness = CalculateFitness(ind.Genes, classi, professori, validTurnIndices);
                population.Add(ind);
            }

            Individual bestSoFar = population.OrderBy(ind => ind.Fitness).First();

            for (int gen = 0; gen < generations; gen++)
            {
                if (bestSoFar.Fitness == 0) break;

                var nextGen = new List<Individual>();
                var sorted = population.OrderBy(ind => ind.Fitness).ToList();
                nextGen.Add(new Individual(sorted[0].Genes) { Fitness = sorted[0].Fitness });
                nextGen.Add(new Individual(sorted[1].Genes) { Fitness = sorted[1].Fitness });

                while (nextGen.Count < populationSize)
                {
                    var parent1 = TournamentSelection(population);
                    var parent2 = TournamentSelection(population);

                    int[] child1Genes, child2Genes;
                    if (_rand.NextDouble() < 0.8)
                    {
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

                    for (int i = 0; i < child1Genes.Length; i++)
                    {
                        if (_rand.NextDouble() < mutationRate) child1Genes[i] = _rand.Next(numTurni);
                    }
                    for (int i = 0; i < child2Genes.Length; i++)
                    {
                        if (_rand.NextDouble() < mutationRate) child2Genes[i] = _rand.Next(numTurni);
                    }

                    var child1 = new Individual(child1Genes);
                    child1.Fitness = CalculateFitness(child1.Genes, classi, professori, validTurnIndices);
                    var child2 = new Individual(child2Genes);
                    child2.Fitness = CalculateFitness(child2.Genes, classi, professori, validTurnIndices);

                    nextGen.Add(child1);
                    if (nextGen.Count < populationSize) nextGen.Add(child2);
                }

                population = nextGen;
                var genBest = population.OrderBy(ind => ind.Fitness).First();
                if (genBest.Fitness < bestSoFar.Fitness)
                {
                    bestSoFar = new Individual(genBest.Genes) { Fitness = genBest.Fitness };
                }
            }

            var resultPiano = PianoScheduling.Inizializza();
            for(int i=0; i<length; i++)
            {
                var vt = validTurnIndices[bestSoFar.Genes[i]];
                var turno = resultPiano.GetTurno(vt.giornata, vt.turno);
                if(turno.Classi.Count < Turno.MaxClassi)
                {
                    turno.AggiungiClasse(classi[i]);
                }
            }
            return resultPiano;
        }

        private Individual TournamentSelection(List<Individual> population)
        {
            var tournament = new List<Individual>();
            for (int i = 0; i < tournamentSize; i++)
            {
                tournament.Add(population[_rand.Next(population.Count)]);
            }
            return tournament.OrderBy(ind => ind.Fitness).First();
        }

        private int CalculateFitness(int[] genes, List<Classe> classi, List<Professore> professori, List<(int giornata, int turno)> validTurnIndices)
        {
            int penalty = 0;
            var numTurni = validTurnIndices.Count;
            
            var classiPerTurno = new int[numTurni];
            for(int i=0; i<genes.Length; i++) classiPerTurno[genes[i]]++;
            
            for(int i=0; i<numTurni; i++)
            {
                if(classiPerTurno[i] > Turno.MaxClassi)
                {
                    penalty += (classiPerTurno[i] - Turno.MaxClassi) * 10000;
                }
            }
            
            var tempPiano = PianoScheduling.Inizializza();
            for(int i=0; i<genes.Length; i++)
            {
                var vt = validTurnIndices[genes[i]];
                var turno = tempPiano.GetTurno(vt.giornata, vt.turno);
                turno.Classi.Add(classi[i]);
            }
            
            penalty += CalcolaScore(tempPiano, professori);
            return penalty;
        }

        private int CalcolaScore(PianoScheduling piano, List<Professore> professori)
        {
            int conflitti = conflictService.ScoreConflittiTotale(piano, professori);
            int prossimita = proximityService.PenalitaTotale(piano, professori);
            return (conflitti * 100) + prossimita;
        }
    }
}