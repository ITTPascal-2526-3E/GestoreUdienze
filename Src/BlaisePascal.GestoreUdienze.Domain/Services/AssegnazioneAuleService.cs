using BlaisePascal.GestoreUdienze.Domain.Entities;
using Google.OrTools.Sat;
using System.Collections.Generic;
using BlaisePascal.GestoreUdienze.Domain.Interfaces;
using System.Linq;

namespace BlaisePascal.GestoreUdienze.Domain.Services
{
    public class AssegnazioneAuleService : IAssegnazioneAuleService
    {
        public IEnumerable<Udienza> Assegna(
            IEnumerable<Classe> classi,
            IEnumerable<Professore> professori,
            IEnumerable<Aula> auleDisponibili,
            IEnumerable<Turno> turni)
        {
            var model = new CpModel();
            
            var classiList = classi.ToList();
            var docentiList = professori.Where(p => p.Attivo).ToList();
            var auleList = auleDisponibili.ToList();
            var turniList = turni.ToList();
            
            int numTurni = turniList.Count;
            int numAule = auleList.Count;
            
            if (numTurni == 0 || numAule == 0 || docentiList.Count == 0 || classiList.Count == 0)
                return new List<Udienza>();

            var udienze = new List<(Professore d, Classe c)>();
            
            var turno_var = new Dictionary<(string, int), IntVar>();
            var aula_var = new Dictionary<(string, int), IntVar>();
            
            foreach (var d in docentiList)
            {
                foreach (var cName in d.ClassiInsegnate)
                {
                    var c = classiList.FirstOrDefault(cl => cl.Nome == cName);
                    if (c != null)
                    {
                        udienze.Add((d, c));
                        var tVar = model.NewIntVar(0, numTurni - 1, $"turno_{d.CodiceProfessore}_{c.Id}");
                        var aVar = model.NewIntVar(0, numAule - 1, $"aula_{d.CodiceProfessore}_{c.Id}");
                        
                        turno_var[(d.CodiceProfessore, c.Id)] = tVar;
                        aula_var[(d.CodiceProfessore, c.Id)] = aVar;
                    }
                }
            }

            // V1: No sovrapposizione docente
            foreach (var d in docentiList)
            {
                var classiInsegnate = udienze.Where(u => u.d.CodiceProfessore == d.CodiceProfessore).Select(u => u.c).ToList();
                var vars = classiInsegnate.Select(c => turno_var[(d.CodiceProfessore, c.Id)]).ToArray();
                if (vars.Length > 1)
                {
                    model.AddAllDifferent(vars);
                }
            }
            
            // V2 & V5: Capacita aule per turno
            for (int t = 0; t < numTurni; t++)
            {
                for (int a = 0; a < numAule; a++)
                {
                    var aulaObj = auleList[a];
                    var presenzeInAula = new List<ILiteral>();
                    
                    foreach (var u in udienze)
                    {
                        var bT = model.NewBoolVar($"is_t{t}_{u.d.CodiceProfessore}_{u.c.Id}");
                        var bA = model.NewBoolVar($"is_a{a}_{u.d.CodiceProfessore}_{u.c.Id}");
                        var bTandA = model.NewBoolVar($"is_ta_{t}_{a}_{u.d.CodiceProfessore}_{u.c.Id}");
                        
                        model.Add(turno_var[(u.d.CodiceProfessore, u.c.Id)] == t).OnlyEnforceIf(bT);
                        model.Add(turno_var[(u.d.CodiceProfessore, u.c.Id)] != t).OnlyEnforceIf(bT.Not());
                        
                        model.Add(aula_var[(u.d.CodiceProfessore, u.c.Id)] == a).OnlyEnforceIf(bA);
                        model.Add(aula_var[(u.d.CodiceProfessore, u.c.Id)] != a).OnlyEnforceIf(bA.Not());
                        
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
                foreach (var c in classiList)
                {
                    var classeInTurno = model.NewBoolVar($"classe_{c.Id}_turno_{t}");
                    var docentiClasse = udienze.Where(u => u.c.Id == c.Id).Select(u => u.d).ToList();
                    
                    foreach (var d in docentiClasse)
                    {
                        var serveClasse = model.NewBoolVar($"serve_{d.CodiceProfessore}_{c.Id}_{t}");
                        model.Add(turno_var[(d.CodiceProfessore, c.Id)] == t).OnlyEnforceIf(serveClasse);
                        model.Add(turno_var[(d.CodiceProfessore, c.Id)] != t).OnlyEnforceIf(serveClasse.Not());
                        
                        model.AddImplication(serveClasse, classeInTurno);
                    }
                }
            }
            
            for (int t = 0; t < numTurni; t++)
            {
                foreach (var d in docentiList)
                {
                    var classiDelDocente = udienze.Where(u => u.d.CodiceProfessore == d.CodiceProfessore).Select(u => u.c).ToList();
                    var attivi = new List<ILiteral>();
                    foreach (var c in classiDelDocente)
                    {
                        var serveClasse = model.NewBoolVar($"attivo_{d.CodiceProfessore}_{c.Id}_{t}");
                        model.Add(turno_var[(d.CodiceProfessore, c.Id)] == t).OnlyEnforceIf(serveClasse);
                        model.Add(turno_var[(d.CodiceProfessore, c.Id)] != t).OnlyEnforceIf(serveClasse.Not());
                        attivi.Add(serveClasse);
                    }
                    if (attivi.Any())
                    {
                        model.Add(LinearExpr.Sum(attivi) <= 1);
                    }
                }
            }
            
            // V4: Docenti di classe nello stesso piano (per i docenti non di laboratorio)
            var pianiArray = auleList.Select(a => (long)a.Piano).ToArray();
            for (int t = 0; t < numTurni; t++)
            {
                foreach (var c in classiList)
                {
                    var docentiC = udienze.Where(u => u.c.Id == c.Id && !u.d.ELaboratorio).ToList();
                    if (docentiC.Count > 1)
                    {
                        var pianoRef = model.NewIntVar(0, 100, $"piano_ref_{c.Id}_{t}");
                        
                        foreach (var u in docentiC)
                        {
                            var d = u.d;
                            var isT = model.NewBoolVar($"v4_{d.CodiceProfessore}_{c.Id}_{t}");
                            model.Add(turno_var[(d.CodiceProfessore, c.Id)] == t).OnlyEnforceIf(isT);
                            model.Add(turno_var[(d.CodiceProfessore, c.Id)] != t).OnlyEnforceIf(isT.Not());
                            
                            var pianoD = model.NewIntVar(0, 100, $"piano_{d.CodiceProfessore}_{c.Id}");
                            model.AddElement(aula_var[(d.CodiceProfessore, c.Id)], pianiArray, pianoD);
                            
                            model.Add(pianoRef == pianoD).OnlyEnforceIf(isT);
                        }
                    }
                }
            }
            
            // P2: Soft constraints S1: Stesso giorno
            var numGiorniDocenteList = new List<IntVar>();
            var giorniDistinct = turniList.Select(tx => tx.Giorno).Distinct().ToList();
            
            foreach (var d in docentiList)
            {
                var udienzeDocente = udienze.Where(u => u.d.CodiceProfessore == d.CodiceProfessore).ToList();
                if (!udienzeDocente.Any()) continue;
                
                var giorniUsatiBool = new List<BoolVar>();
                for (int g = 0; g < giorniDistinct.Count; g++)
                {
                    var gBool = model.NewBoolVar($"giorno_{d.CodiceProfessore}_{g}");
                    giorniUsatiBool.Add(gBool);
                    
                    var turniInGiorno = turniList.Select((tx, idx) => new { tx, idx })
                        .Where(x => x.tx.Giorno == giorniDistinct[g])
                        .Select(x => x.idx).ToList();
                    
                    var presentiInGiorno = new List<ILiteral>();
                    foreach (var u in udienzeDocente)
                    {
                        foreach (var tIdx in turniInGiorno)
                        {
                            var isT = model.NewBoolVar($"is_tg_{d.CodiceProfessore}_{u.c.Id}_{tIdx}");
                            model.Add(turno_var[(d.CodiceProfessore, u.c.Id)] == tIdx).OnlyEnforceIf(isT);
                            model.Add(turno_var[(d.CodiceProfessore, u.c.Id)] != tIdx).OnlyEnforceIf(isT.Not());
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
            
            // Objective function
            if (numGiorniDocenteList.Any())
            {
                var penalty_giorni_multipli = LinearExpr.Sum(numGiorniDocenteList);
                model.Minimize(penalty_giorni_multipli);
            }
            
            // Solve
            var solver = new CpSolver();
            solver.StringParameters = "max_time_in_seconds: 300.0";
            var status = solver.Solve(model);
            
            var risultato = new List<Udienza>();
            if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
            {
                foreach (var u in udienze)
                {
                    int assignedT = (int)solver.Value(turno_var[(u.d.CodiceProfessore, u.c.Id)]);
                    int assignedA = (int)solver.Value(aula_var[(u.d.CodiceProfessore, u.c.Id)]);
                    
                    risultato.Add(new Udienza
                    {
                        Docente = u.d,
                        Classe = u.c,
                        Turno = turniList[assignedT],
                        Aula = auleList[assignedA]
                    });
                }
            }
            
            return risultato;
        }
    }
}
