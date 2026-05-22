using BlaisePascal.GestoreUdienze.Application.Scheduling.Models;
using Google.OrTools.Sat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BlaisePascal.GestoreUdienze.Application.Scheduling
{
    // Motore di schedulazione principale basato su Google OR-Tools CP-SAT.
    // Gestisce l'intera complessità del problema: turni, aule, vincoli hard (P1) e soft (P2).
    // 
    public class UdienzeSchedulerService
    {
        private readonly int _maxTimeSeconds;

        public UdienzeSchedulerService(int maxTimeSeconds = 300)
        {
            _maxTimeSeconds = maxTimeSeconds;
        }

        public RisultatoSchedulingDto Risolvi(
            List<DocenteDto> docenti,
            List<ClasseDto> classi,
            List<AulaDto> aule,
            List<TurnoDto> turni)
        {
            var risultato = new RisultatoSchedulingDto();
            var sw = Stopwatch.StartNew();

            // Filtra solo docenti attivi
            var docentiAttivi = docenti.Where(d => d.Attivo).ToList();

            if (!docentiAttivi.Any() || !classi.Any() || !aule.Any() || !turni.Any())
            {
                risultato.StatoSolver = "Infeasible";
                risultato.Warnings.Add("Input insufficiente: verificare docenti attivi, classi, aule e turni.");
                risultato.TempoRisoluzioneSec = sw.Elapsed.TotalSeconds;
                return risultato;
            }

            int numTurni = turni.Count;
            int numAule = aule.Count;

            var model = new CpModel();

            // --- Costruzione delle udienze (docente, classe) ---
            var udienze = new List<(DocenteDto d, ClasseDto c)>();
            var turno_var = new Dictionary<(string, int), IntVar>();
            var aula_var = new Dictionary<(string, int), IntVar>();

            foreach (var d in docentiAttivi)
            {
                foreach (var cName in d.ClassiInsegnate)
                {
                    var c = classi.FirstOrDefault(cl => cl.Nome == cName);
                    if (c != null)
                    {
                        udienze.Add((d, c));
                        var key = (d.CodiceProfessore, c.Id);
                        turno_var[key] = model.NewIntVar(0, numTurni - 1, $"turno_{d.CodiceProfessore}_{c.Id}");
                        aula_var[key] = model.NewIntVar(0, numAule - 1, $"aula_{d.CodiceProfessore}_{c.Id}");
                    }
                }
            }

            if (!udienze.Any())
            {
                risultato.StatoSolver = "Infeasible";
                risultato.Warnings.Add("Nessuna udienza generata: nessun docente attivo insegna nelle classi fornite.");
                risultato.TempoRisoluzioneSec = sw.Elapsed.TotalSeconds;
                return risultato;
            }


            // VINCOLI P1 — HARD (obbligatori)

            // V1: No sovrapposizione docente (un docente non può essere in due posti nello stesso turno)
            foreach (var d in docentiAttivi)
            {
                var vars = udienze
                    .Where(u => u.d.CodiceProfessore == d.CodiceProfessore)
                    .Select(u => turno_var[(d.CodiceProfessore, u.c.Id)])
                    .ToArray();
                if (vars.Length > 1)
                {
                    model.AddAllDifferent(vars);
                }
            }

            // V2 & V5: Capacità aule per turno (massimo CapacitaMaterie docenti per aula per turno)
            for (int t = 0; t < numTurni; t++)
            {
                for (int a = 0; a < numAule; a++)
                {
                    var aulaObj = aule[a];
                    var presenzeInAula = new List<ILiteral>();

                    foreach (var u in udienze)
                    {
                        var key = (u.d.CodiceProfessore, u.c.Id);
                        var bT = model.NewBoolVar($"is_t{t}_{key.Item1}_{key.Item2}");
                        var bA = model.NewBoolVar($"is_a{a}_{key.Item1}_{key.Item2}");
                        var bTandA = model.NewBoolVar($"is_ta_{t}_{a}_{key.Item1}_{key.Item2}");

                        model.Add(turno_var[key] == t).OnlyEnforceIf(bT);
                        model.Add(turno_var[key] != t).OnlyEnforceIf(bT.Not());

                        model.Add(aula_var[key] == a).OnlyEnforceIf(bA);
                        model.Add(aula_var[key] != a).OnlyEnforceIf(bA.Not());

                        model.AddBoolAnd(new[] { bT, bA }).OnlyEnforceIf(bTandA);
                        model.AddBoolOr(new[] { bT.Not(), bA.Not() }).OnlyEnforceIf(bTandA.Not());

                        presenzeInAula.Add(bTandA);
                    }
                    if (presenzeInAula.Any())
                    {
                        model.Add(LinearExpr.Sum(presenzeInAula) <= aulaObj.CapacitaMaterie);
                    }
                }
            }

            // V3: Un docente per turno ha udienze di UNA SOLA classe
            for (int t = 0; t < numTurni; t++)
            {
                foreach (var d in docentiAttivi)
                {
                    var classiDelDocente = udienze
                        .Where(u => u.d.CodiceProfessore == d.CodiceProfessore)
                        .Select(u => u.c)
                        .ToList();

                    var attivi = new List<ILiteral>();
                    foreach (var c in classiDelDocente)
                    {
                        var key = (d.CodiceProfessore, c.Id);
                        var serveClasse = model.NewBoolVar($"attivo_{key.Item1}_{key.Item2}_{t}");
                        model.Add(turno_var[key] == t).OnlyEnforceIf(serveClasse);
                        model.Add(turno_var[key] != t).OnlyEnforceIf(serveClasse.Not());
                        attivi.Add(serveClasse);
                    }
                    if (attivi.Count > 1)
                    {
                        model.Add(LinearExpr.Sum(attivi) <= 1);
                    }
                }
            }

            // V4: Docenti di classe nello stesso piano (per docenti NON di laboratorio)
            var pianiArray = aule.Select(a => (long)a.Piano).ToArray();
            for (int t = 0; t < numTurni; t++)
            {
                foreach (var c in classi)
                {
                    var docentiC = udienze
                        .Where(u => u.c.Id == c.Id && !u.d.IsLaboratorio)
                        .ToList();

                    if (docentiC.Count > 1)
                    {
                        var pianoRef = model.NewIntVar(0, 100, $"piano_ref_{c.Id}_{t}");

                        foreach (var u in docentiC)
                        {
                            var key = (u.d.CodiceProfessore, u.c.Id);
                            var isT = model.NewBoolVar($"v4_{key.Item1}_{key.Item2}_{t}");
                            model.Add(turno_var[key] == t).OnlyEnforceIf(isT);
                            model.Add(turno_var[key] != t).OnlyEnforceIf(isT.Not());

                            var pianoD = model.NewIntVar(0, 100, $"piano_{key.Item1}_{key.Item2}");
                            model.AddElement(aula_var[key], pianiArray, pianoD);

                            model.Add(pianoRef == pianoD).OnlyEnforceIf(isT);
                        }
                    }
                }
            }

            // V6: Docenti di laboratorio — nessun vincolo di posizione (V4 non applicato, gestito sopra)
            // V7: Solo docenti attivi — gestito dal filtro iniziale

            //    
            // VINCOLI P2 — SOFT (preferenziali, come funzione obiettivo)

            // S1: Stesso giorno per tutte le udienze di un docente
            var numGiorniDocenteList = new List<IntVar>();
            var giorniDistinct = turni.Select(tx => tx.Giorno).Distinct().ToList();

            foreach (var d in docentiAttivi)
            {
                var udienzeDocente = udienze
                    .Where(u => u.d.CodiceProfessore == d.CodiceProfessore)
                    .ToList();
                if (!udienzeDocente.Any()) continue;

                var giorniUsatiBool = new List<BoolVar>();
                for (int g = 0; g < giorniDistinct.Count; g++)
                {
                    var gBool = model.NewBoolVar($"giorno_{d.CodiceProfessore}_{g}");
                    giorniUsatiBool.Add(gBool);

                    var turniInGiorno = turni
                        .Select((tx, idx) => new { tx, idx })
                        .Where(x => x.tx.Giorno == giorniDistinct[g])
                        .Select(x => x.idx)
                        .ToList();

                    var presentiInGiorno = new List<ILiteral>();
                    foreach (var u in udienzeDocente)
                    {
                        var key = (d.CodiceProfessore, u.c.Id);
                        foreach (var tIdx in turniInGiorno)
                        {
                            var isT = model.NewBoolVar($"is_tg_{key.Item1}_{key.Item2}_{tIdx}");
                            model.Add(turno_var[key] == tIdx).OnlyEnforceIf(isT);
                            model.Add(turno_var[key] != tIdx).OnlyEnforceIf(isT.Not());
                            presentiInGiorno.Add(isT);
                        }
                    }

                    if (presentiInGiorno.Any())
                    {
                        model.Add(LinearExpr.Sum(presentiInGiorno) > 0).OnlyEnforceIf(gBool);
                        model.Add(LinearExpr.Sum(presentiInGiorno) == 0).OnlyEnforceIf(gBool.Not());
                    }
                    else
                    {
                        model.Add(gBool == 0);
                    }
                }

                var numGiorniD = model.NewIntVar(0, giorniDistinct.Count, $"numGiorni_{d.CodiceProfessore}");
                model.Add(numGiorniD == LinearExpr.Sum(giorniUsatiBool));
                numGiorniDocenteList.Add(numGiorniD);
            }

            // Funzione obiettivo: minimizzare la dispersione dei giorni
            if (numGiorniDocenteList.Any())
            {
                model.Minimize(LinearExpr.Sum(numGiorniDocenteList));
            }

            // RISOLUZIONE

            var solver = new CpSolver();
            solver.StringParameters = $"max_time_in_seconds: {_maxTimeSeconds}.0";
            var status = solver.Solve(model);

            sw.Stop();
            risultato.TempoRisoluzioneSec = sw.Elapsed.TotalSeconds;
            risultato.StatoSolver = status.ToString();

            if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
            {
                risultato.ValoreObiettivo = solver.ObjectiveValue;

                foreach (var u in udienze)
                {
                    var key = (u.d.CodiceProfessore, u.c.Id);
                    int assignedT = (int)solver.Value(turno_var[key]);
                    int assignedA = (int)solver.Value(aula_var[key]);

                    risultato.Udienze.Add(new UdienzaAssegnataDto
                    {
                        CodiceProfessore = u.d.CodiceProfessore,
                        NomeProfessore = $"{u.d.Nome} {u.d.Cognome}",
                        ClasseId = u.c.Id,
                        ClasseNome = u.c.Nome,
                        TurnoId = turni[assignedT].Id,
                        TurnoGiorno = turni[assignedT].Giorno,
                        AulaId = aule[assignedA].Id,
                        AulaNome = aule[assignedA].Nome,
                        AulaPiano = aule[assignedA].Piano
                    });
                }

                if (status == CpSolverStatus.Feasible)
                {
                    risultato.Warnings.Add("Soluzione trovata ma potenzialmente non ottimale (timeout raggiunto o solver interrotto).");
                }
            }
            else
            {
                risultato.Warnings.Add($"Il solver non ha trovato una soluzione. Status: {status}");
            }

            return risultato;
        }
    }
}
