using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Microsoft.Data.Sqlite;
using BlaisePascal.GestoreUdienze.Domain.Models;
using BlaisePascal.GestoreUdienze.Application.Scheduling;
using BlaisePascal.GestoreUdienze.Application.Scheduling.Models;

namespace BlaisePascal.GestoreUdienze.Wpf
{
    public partial class MainWindow : Window
    {
        private string _caricatoFilePath = string.Empty;
        private string _caricatoPdfPath = string.Empty;
        private string _connectionString = "Data Source=gestoreudienze.db";
        private RisultatoSchedulingDto? _ultimoRisultatoScheduling = null;
        private DateTime _dataInizioScheduling = DateTime.Now;

        public MainWindow()
        {
            InitializeComponent();
            AssegnaEventi();
            InizializzaInterfaccia();
        }

        private void InizializzaInterfaccia()
        {
            DatePickerGiornata.SelectedDate = DateTime.Now;
            
            try
            {
                EnsureDatabaseInitialized();
                CaricaDatiDalDatabase();
                SetStatoControlli(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nell'inizializzazione del database: {ex.Message}", "Errore Database", MessageBoxButton.OK, MessageBoxImage.Error);
                SetStatoControlli(false);
            }
        }

        private void AssegnaEventi()
        {
            BtnBrowse.Click += BtnBrowse_Click;
            BtnBrowsePdf.Click += BtnBrowsePdf_Click;
            BtnStampaGiornata.Click += BtnStampaGiornata_Click;
            BtnStampaAule.Click += BtnStampaAule_Click;
            BtnStampaClassi.Click += BtnStampaClassi_Click;
            BtnGenera.Click += BtnGenera_Click;
        }

        private void SetStatoControlli(bool isAbilitato)
        {
            DatePickerGiornata.IsEnabled = isAbilitato;
            ListAule.IsEnabled = isAbilitato;
            ListClassi.IsEnabled = isAbilitato;

            BtnStampaGiornata.IsEnabled = isAbilitato;
            BtnStampaAule.IsEnabled = isAbilitato;
            BtnStampaClassi.IsEnabled = isAbilitato;
            BtnGenera.IsEnabled = isAbilitato;
        }

        private void EnsureDatabaseInitialized()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Create Professori
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Professori (
                    CodiceProfessore TEXT PRIMARY KEY,
                    Nome TEXT,
                    Cognome TEXT,
                    IsLaboratorio INTEGER DEFAULT 0,
                    Attivo INTEGER DEFAULT 1
                );";
                cmd.ExecuteNonQuery();
            }

            // Create Aule
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Aule (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nome TEXT,
                    Ala TEXT,
                    Piano INTEGER
                );";
                cmd.ExecuteNonQuery();
            }

            // Create Classi
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Classi (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nome TEXT,
                    CodiceProfessore TEXT
                );";
                cmd.ExecuteNonQuery();
            }

            // Create Materie
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Materie (
                    CodiceMateria TEXT PRIMARY KEY,
                    NomeMateria TEXT,
                    CodiceProfessore TEXT
                );";
                cmd.ExecuteNonQuery();
            }

            // Create OrarioTurni
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS OrarioTurni (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Orario INTEGER,
                    NomeProfessore TEXT,
                    CognomeProfessore TEXT
                );";
                cmd.ExecuteNonQuery();
            }

            // Seed Aule if empty
            bool hasAule = false;
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM Aule;";
                hasAule = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
            if (!hasAule)
            {
                using (var transaction = connection.BeginTransaction())
                {
                    for (int i = 1; i <= 50; i++)
                    {
                        using (var insertCmd = connection.CreateCommand())
                        {
                            insertCmd.CommandText = "INSERT INTO Aule (Nome, Ala, Piano) VALUES (@nome, @ala, @piano);";
                            insertCmd.Parameters.AddWithValue("@nome", $"Aula {i}");
                            insertCmd.Parameters.AddWithValue("@ala", i <= 25 ? "Ala A" : "Ala B");
                            insertCmd.Parameters.AddWithValue("@piano", i <= 25 ? 0 : 1);
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
            }

            // Seed Classi if empty
            bool hasClassi = false;
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM Classi;";
                hasClassi = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
            if (!hasClassi)
            {
                using (var transaction = connection.BeginTransaction())
                {
                    string[] sezioni = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "L", "M", "N" };
                    for (int anno = 1; anno <= 5; anno++)
                    {
                        foreach (string sezione in sezioni)
                        {
                            using (var insertCmd = connection.CreateCommand())
                            {
                                insertCmd.CommandText = "INSERT INTO Classi (Nome, CodiceProfessore) VALUES (@nome, @codiceProfessore);";
                                insertCmd.Parameters.AddWithValue("@nome", $"{anno}{sezione}");
                                insertCmd.Parameters.AddWithValue("@codiceProfessore", "");
                                insertCmd.ExecuteNonQuery();
                            }
                        }
                        using (var insertCmdBio = connection.CreateCommand())
                        {
                            insertCmdBio.CommandText = "INSERT INTO Classi (Nome, CodiceProfessore) VALUES (@nome, @codiceProfessore);";
                            insertCmdBio.Parameters.AddWithValue("@nome", $"{anno}BIO");
                            insertCmdBio.Parameters.AddWithValue("@codiceProfessore", "");
                            insertCmdBio.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
            }
        }

        private void CaricaDatiDalDatabase()
        {
            var aule = LeggiAuleDalDatabase();
            var classi = LeggiClassiDalDatabase();

            ListAule.ItemsSource = aule;
            ListClassi.ItemsSource = classi;
        }

        private List<Aula> LeggiAuleDalDatabase()
        {
            var aule = new List<Aula>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, Nome, Ala, Piano FROM Aule ORDER BY Nome;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                aule.Add(new Aula
                {
                    Id = reader.GetInt32(0),
                    Nome = reader.GetString(1),
                    Ala = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Piano = reader.IsDBNull(3) ? 0 : reader.GetInt32(3)
                });
            }
            return aule;
        }

        private List<Classe> LeggiClassiDalDatabase()
        {
            var classi = new List<Classe>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, Nome, CodiceProfessore FROM Classi ORDER BY Nome;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var cl = new Classe
                {
                    Id = reader.GetInt32(0)
                };
                cl.ImpostaNome(reader.GetString(1));
                if (!reader.IsDBNull(2))
                {
                    cl.CodiceProfessore = reader.GetString(2);
                }
                classi.Add(cl);
            }
            return classi;
        }

        private List<Professore> LeggiProfessoriDalDatabase()
        {
            var professori = new List<Professore>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Legge i docenti
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT CodiceProfessore, Nome, Cognome, IsLaboratorio, Attivo FROM Professori;";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    professori.Add(new Professore
                    {
                        CodiceProfessore = reader.GetString(0),
                        Nome = reader.GetString(1),
                        Cognome = reader.GetString(2),
                        IsLaboratorio = reader.GetInt32(3) == 1,
                        Attivo = reader.GetInt32(4) == 1
                    });
                }
            }

            // Associa le classi a ciascun docente
            foreach (var p in professori)
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT Id, Nome FROM Classi WHERE CodiceProfessore = @codice;";
                cmd.Parameters.AddWithValue("@codice", p.CodiceProfessore);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var cl = new Classe
                    {
                        Id = reader.GetInt32(0),
                        CodiceProfessore = p.CodiceProfessore
                    };
                    cl.ImpostaNome(reader.GetString(1));
                    p.Classi.Add(cl);
                }
            }

            return professori;
        }

        // Sfoglia File Dati (Excel / CSV)
        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Seleziona il file dei dati delle udienze",
                Filter = "File Excel (*.xlsx;*.xls)|*.xlsx;*.xls|File CSV (*.csv)|*.csv",
                FilterIndex = 2, // Preferiamo CSV per una lettura nativa e affidabile
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _caricatoFilePath = openFileDialog.FileName;
                TxtFilePath.Text = Path.GetFileName(_caricatoFilePath);
                TxtFilePath.Foreground = System.Windows.Media.Brushes.Black;

                try
                {
                    string est = Path.GetExtension(_caricatoFilePath).ToLower();
                    if (est == ".csv")
                    {
                        ImportaCsv(_caricatoFilePath);
                    }
                    else
                    {
                        MessageBox.Show("Il parser nativo supporta file CSV. Per file Excel convertili prima in formato CSV con separatore virgola (,) o punto e virgola (;).", "Informativa Formato", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    CaricaDatiDalDatabase();
                    SetStatoControlli(true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore durante la lettura del file dati: {ex.Message}", "Errore di Caricamento", MessageBoxButton.OK, MessageBoxImage.Error);
                    SetStatoControlli(false);
                }
            }
        }

        private void ImportaCsv(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            if (lines.Length == 0) return;

            bool hasHeader = lines[0].Contains("Codice") || lines[0].Contains("Cognome") || lines[0].Contains("Nome") || lines[0].Contains("Classe");
            int startRow = hasHeader ? 1 : 0;

            var professori = new List<Professore>();
            var classi = new List<Classe>();
            var materie = new List<Materia>();

            for (int i = startRow; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(new[] { ',', ';' });
                if (parts.Length < 3) continue;

                string codice = parts[0].Trim();
                string cognome = parts[1].Trim();
                string nome = parts[2].Trim();
                bool isLab = false;
                if (parts.Length > 3)
                {
                    bool.TryParse(parts[3].Trim(), out isLab);
                }

                var prof = professori.FirstOrDefault(p => p.CodiceProfessore.Equals(codice, StringComparison.OrdinalIgnoreCase));
                if (prof == null)
                {
                    prof = new Professore(codice, nome, cognome, false, true, isLab);
                    professori.Add(prof);
                }

                if (parts.Length > 4)
                {
                    string classeNome = parts[4].Trim();
                    if (!string.IsNullOrEmpty(classeNome))
                    {
                        var cls = classi.FirstOrDefault(c => c.Nome.Equals(classeNome, StringComparison.OrdinalIgnoreCase));
                        if (cls == null)
                        {
                            cls = new Classe(classi.Count + 1, classeNome, codice);
                            classi.Add(cls);
                        }
                        if (!prof.Classi.Any(c => c.Nome.Equals(classeNome, StringComparison.OrdinalIgnoreCase)))
                        {
                            prof.Classi.Add(cls);
                        }
                    }
                }

                if (parts.Length > 5)
                {
                    string materiaNome = parts[5].Trim();
                    if (!string.IsNullOrEmpty(materiaNome))
                    {
                        var mat = materie.FirstOrDefault(m => m.NomeMateria.Equals(materiaNome, StringComparison.OrdinalIgnoreCase) && m.CodiceProfessore == codice);
                        if (mat == null)
                        {
                            mat = new Materia
                            {
                                CodiceMateria = $"M_{codice}_{materiaNome.Replace(" ", "_")}",
                                NomeMateria = materiaNome,
                                CodiceProfessore = codice
                            };
                            materie.Add(mat);
                        }
                    }
                }
            }

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Svuota le tabelle in ordine di vincolo
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM OrarioTurni; DELETE FROM Materie; DELETE FROM Classi; DELETE FROM Professori;";
                    cmd.ExecuteNonQuery();
                }

                // Inserimento Professori
                foreach (var p in professori)
                {
                    using var cmd = connection.CreateCommand();
                    cmd.CommandText = @"
                    INSERT INTO Professori (CodiceProfessore, Nome, Cognome, IsLaboratorio, Attivo)
                    VALUES (@codice, @nome, @cognome, @isLab, 1);";
                    cmd.Parameters.AddWithValue("@codice", p.CodiceProfessore);
                    cmd.Parameters.AddWithValue("@nome", p.Nome);
                    cmd.Parameters.AddWithValue("@cognome", p.Cognome);
                    cmd.Parameters.AddWithValue("@isLab", p.IsLaboratorio ? 1 : 0);
                    cmd.ExecuteNonQuery();
                }

                // Inserimento Classi
                foreach (var c in classi)
                {
                    using var cmd = connection.CreateCommand();
                    cmd.CommandText = @"
                    INSERT INTO Classi (Nome, CodiceProfessore)
                    VALUES (@nome, @codiceProfessore);";
                    cmd.Parameters.AddWithValue("@nome", c.Nome);
                    cmd.Parameters.AddWithValue("@codiceProfessore", c.CodiceProfessore);
                    cmd.ExecuteNonQuery();
                }

                // Inserimento Materie
                foreach (var m in materie)
                {
                    using var cmd = connection.CreateCommand();
                    cmd.CommandText = @"
                    INSERT INTO Materie (CodiceMateria, NomeMateria, CodiceProfessore)
                    VALUES (@codice, @nome, @codiceProfessore);";
                    cmd.Parameters.AddWithValue("@codice", m.CodiceMateria);
                    cmd.Parameters.AddWithValue("@nome", m.NomeMateria);
                    cmd.Parameters.AddWithValue("@codiceProfessore", m.CodiceProfessore);
                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
                MessageBox.Show($"Importazione completata con successo! Importati {professori.Count} professori, {classi.Count} classi e {materie.Count} materie.", "Importazione Successo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Errore nel salvataggio dei dati nel database: {ex.Message}", ex);
            }
        }

        // Sfoglia File PDF
        private void BtnBrowsePdf_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Seleziona il file verbale PDF",
                Filter = "Documenti PDF (*.pdf)|*.pdf",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _caricatoPdfPath = openFileDialog.FileName;
                TxtPdfPath.Text = Path.GetFileName(_caricatoPdfPath);
                TxtPdfPath.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private async void BtnGenera_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_caricatoPdfPath))
            {
                MessageBoxResult result = MessageBox.Show("Attenzione: nessun file PDF selezionato. Procedere comunque?", "Verifica Documento", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            // Prepariamo l'interfaccia
            BtnGenera.IsEnabled = false;
            PanelLoading.Visibility = Visibility.Visible;
            ProgressBarGenera.Value = 10;
            TxtPercentuale.Text = "10%";

            _dataInizioScheduling = DatePickerGiornata.SelectedDate ?? DateTime.Now;

            try
            {
                // Carica i dati aggiornati
                var aule = LeggiAuleDalDatabase();
                var classi = LeggiClassiDalDatabase();
                var professori = LeggiProfessoriDalDatabase();

                ProgressBarGenera.Value = 30;
                TxtPercentuale.Text = "30%";

                if (professori.Count == 0)
                {
                    MessageBox.Show("Nessun professore registrato nel database. Caricare prima un file dati CSV valido.", "Dati Mancanti", MessageBoxButton.OK, MessageBoxImage.Warning);
                    PanelLoading.Visibility = Visibility.Collapsed;
                    BtnGenera.IsEnabled = true;
                    return;
                }

                // Costruisci i DTO per UdienzeSchedulerService
                var auleDtos = aule.Select(a => new AulaDto
                {
                    Id = a.Id,
                    Nome = a.Nome,
                    Ala = a.Ala,
                    Piano = a.Piano,
                    CapacitaMaterie = a.CapacitaMaterie
                }).ToList();

                var classiDtos = classi.Select(c => {
                    var docentiIds = new List<string>();
                    if (!string.IsNullOrEmpty(c.CodiceProfessore))
                    {
                        docentiIds.Add(c.CodiceProfessore);
                    }
                    return new ClasseDto
                    {
                        Id = c.Id,
                        Nome = c.Nome,
                        DocentiIds = docentiIds
                    };
                }).ToList();

                var docentiDtos = professori.Select(p => new DocenteDto
                {
                    CodiceProfessore = p.CodiceProfessore,
                    Nome = p.Nome,
                    Cognome = p.Cognome,
                    Attivo = p.Attivo,
                    IsLaboratorio = p.IsLaboratorio,
                    ClassiInsegnate = p.Classi.Select(cl => cl.Nome).ToList()
                }).ToList();

                // 16 Turni (4 Giorni x 4 Turni al giorno)
                var turniDtos = new List<TurnoDto>();
                int turnoId = 1;
                string[] giorni = { "Giorno 1", "Giorno 2", "Giorno 3", "Giorno 4" };
                TimeSpan[] orariInizio = { new TimeSpan(8, 0, 0), new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0) };
                TimeSpan[] orariFine = { new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0), new TimeSpan(12, 0, 0) };

                for (int g = 0; g < giorni.Length; g++)
                {
                    for (int t = 0; t < 4; t++)
                    {
                        turniDtos.Add(new TurnoDto
                        {
                            Id = turnoId++,
                            Giorno = giorni[g],
                            OraInizio = orariInizio[t],
                            OraFine = orariFine[t]
                        });
                    }
                }

                ProgressBarGenera.Value = 50;
                TxtPercentuale.Text = "50%";

                // Risoluzione asincrona del solver CP-SAT
                var service = new UdienzeSchedulerService(30);
                var risultato = await Task.Run(() => {
                    return service.Risolvi(docentiDtos, classiDtos, auleDtos, turniDtos);
                });

                ProgressBarGenera.Value = 90;
                TxtPercentuale.Text = "90%";

                if (risultato.Successo)
                {
                    _ultimoRisultatoScheduling = risultato;
                    ProgressBarGenera.Value = 100;
                    TxtPercentuale.Text = "100%";

                    MessageBox.Show($"Generazione completata con successo!\nStato Solver: {risultato.StatoSolver}\nTempo Risoluzione: {risultato.TempoRisoluzioneSec:F2} secondi\nUdienze Assegnate: {risultato.Udienze.Count}", "Elaborazione Completata", MessageBoxButton.OK, MessageBoxImage.Information);

                    SalvaReportGenerale(risultato);
                }
                else
                {
                    string warnings = string.Join("\n", risultato.Warnings);
                    MessageBox.Show($"Il solver non è riuscito a trovare una soluzione valida.\nStato: {risultato.StatoSolver}\nDiagnostica:\n{warnings}", "Soluzione Non Trovata", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore imprevisto durante l'elaborazione: {ex.Message}", "Errore di Elaborazione", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                PanelLoading.Visibility = Visibility.Collapsed;
                BtnGenera.IsEnabled = true;
            }
        }

        private void SalvaReportGenerale(RisultatoSchedulingDto risultato)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Salva Report Completo Udienze",
                Filter = "File di Testo (*.txt)|*.txt|File CSV (*.csv)|*.csv",
                FileName = "Report_Generale_Udienze.txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string est = Path.GetExtension(saveFileDialog.FileName).ToLower();
                    if (est == ".csv")
                    {
                        var sb = new System.Text.StringBuilder();
                        sb.AppendLine("Giorno;Ora;Professore;Classe;Aula;Piano");
                        foreach (var u in risultato.Udienze.OrderBy(x => x.TurnoId))
                        {
                            var turno = CercaTurno(u.TurnoId);
                            string oraStr = turno != null ? $"{turno.OraInizio:hh\\:mm}-{turno.OraFine:hh\\:mm}" : "";
                            sb.AppendLine($"{u.TurnoGiorno};{oraStr};{u.NomeProfessore};{u.ClasseNome};{u.AulaNome};{u.AulaPiano}");
                        }
                        File.WriteAllText(saveFileDialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                    }
                    else
                    {
                        var sb = new System.Text.StringBuilder();
                        sb.AppendLine("=================================================================================");
                        sb.AppendLine("                         REPORT GENERALE UDIEZE ASSEGNATE                        ");
                        sb.AppendLine("=================================================================================");
                        sb.AppendLine($"Stato Solver: {risultato.StatoSolver}");
                        sb.AppendLine($"Tempo Risoluzione: {risultato.TempoRisoluzioneSec:F2} secondi");
                        sb.AppendLine($"Numero Udienze Assegnate: {risultato.Udienze.Count}");
                        sb.AppendLine("---------------------------------------------------------------------------------");
                        sb.AppendLine(string.Format("{0,-12} | {1,-12} | {2,-25} | {3,-10} | {4,-10}", "Giorno", "Orario", "Professore", "Classe", "Aula (Piano)"));
                        sb.AppendLine("---------------------------------------------------------------------------------");

                        foreach (var u in risultato.Udienze.OrderBy(x => x.TurnoId))
                        {
                            var turno = CercaTurno(u.TurnoId);
                            string oraStr = turno != null ? $"{turno.OraInizio:hh\\:mm}-{turno.OraFine:hh\\:mm}" : "";
                            sb.AppendLine(string.Format("{0,-12} | {1,-12} | {2,-25} | {3,-10} | {4,-10}", 
                                u.TurnoGiorno, 
                                oraStr, 
                                u.NomeProfessore, 
                                u.ClasseNome, 
                                $"{u.AulaNome} (P{u.AulaPiano})"));
                        }
                        sb.AppendLine("=================================================================================");
                        File.WriteAllText(saveFileDialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                    }
                    MessageBox.Show("Report salvato con successo!", "Salvataggio", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore durante il salvataggio: {ex.Message}", "Errore Salvataggio", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private TurnoDto? CercaTurno(int turnoId)
        {
            int id = 1;
            string[] giorni = { "Giorno 1", "Giorno 2", "Giorno 3", "Giorno 4" };
            TimeSpan[] orariInizio = { new TimeSpan(8, 0, 0), new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0) };
            TimeSpan[] orariFine = { new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0), new TimeSpan(12, 0, 0) };

            for (int g = 0; g < giorni.Length; g++)
            {
                for (int t = 0; t < 4; t++)
                {
                    if (id == turnoId)
                    {
                        return new TurnoDto
                        {
                            Id = id,
                            Giorno = giorni[g],
                            OraInizio = orariInizio[t],
                            OraFine = orariFine[t]
                        };
                    }
                    id++;
                }
            }
            return null;
        }

        private void BtnStampaGiornata_Click(object sender, RoutedEventArgs e)
        {
            if (_ultimoRisultatoScheduling == null)
            {
                MessageBox.Show("Nessuno scheduling generato. Cliccare prima su GENERA.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime? dataSelezionata = DatePickerGiornata.SelectedDate;
            if (!dataSelezionata.HasValue)
            {
                MessageBox.Show("Selezionare una data valida.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int diffDays = (dataSelezionata.Value.Date - _dataInizioScheduling.Date).Days;
            if (diffDays < 0 || diffDays >= 4)
            {
                MessageBox.Show($"La data selezionata ({dataSelezionata.Value.ToShortDateString()}) non rientra nei 4 giorni dello scheduling generato (dal {_dataInizioScheduling.ToShortDateString()} al {_dataInizioScheduling.AddDays(3).ToShortDateString()}).", "Errore Data", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string giornoCercato = $"Giorno {diffDays + 1}";
            var udienzeFiltrate = _ultimoRisultatoScheduling.Udienze.Where(u => u.TurnoGiorno == giornoCercato).ToList();

            if (udienzeFiltrate.Count == 0)
            {
                MessageBox.Show("Nessuna udienza programmata per questa giornata.", "Informazione", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = $"Salva Report Giornata {giornoCercato} ({dataSelezionata.Value.ToShortDateString()})",
                Filter = "File di Testo (*.txt)|*.txt|File CSV (*.csv)|*.csv",
                FileName = $"Report_Udienze_{dataSelezionata.Value:yyyyMMdd}.txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                EsportaUdienzeAFile(udienzeFiltrate, saveFileDialog.FileName, $"Report Giornata - {dataSelezionata.Value.ToShortDateString()} ({giornoCercato})");
            }
        }

        private void BtnStampaAule_Click(object sender, RoutedEventArgs e)
        {
            if (_ultimoRisultatoScheduling == null)
            {
                MessageBox.Show("Nessuno scheduling generato. Cliccare prima su GENERA.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var auleSelezionate = new List<string>();
            foreach (var item in ListAule.SelectedItems)
            {
                if (item is Aula a)
                {
                    auleSelezionate.Add(a.Nome);
                }
            }

            if (auleSelezionate.Count == 0)
            {
                MessageBox.Show("Selezionare almeno un'aula dalla lista.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var udienzeFiltrate = _ultimoRisultatoScheduling.Udienze.Where(u => auleSelezionate.Contains(u.AulaNome)).ToList();

            if (udienzeFiltrate.Count == 0)
            {
                MessageBox.Show("Nessuna udienza programmata per le aule selezionate.", "Informazione", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Salva Report Aule Selezionate",
                Filter = "File di Testo (*.txt)|*.txt|File CSV (*.csv)|*.csv",
                FileName = "Report_Udienze_Aule.txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                EsportaUdienzeAFile(udienzeFiltrate, saveFileDialog.FileName, $"Report Aule - {string.Join(", ", auleSelezionate)}");
            }
        }

        private void BtnStampaClassi_Click(object sender, RoutedEventArgs e)
        {
            if (_ultimoRisultatoScheduling == null)
            {
                MessageBox.Show("Nessuno scheduling generato. Cliccare prima su GENERA.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ListClassi.SelectedItem == null)
            {
                MessageBox.Show("Selezionare una classe dalla lista.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string classeSelezionata = string.Empty;
            if (ListClassi.SelectedItem is Classe c)
            {
                classeSelezionata = c.Nome;
            }
            else
            {
                classeSelezionata = ListClassi.SelectedItem?.ToString() ?? string.Empty;
            }

            var udienzeFiltrate = _ultimoRisultatoScheduling.Udienze.Where(u => u.ClasseNome.Equals(classeSelezionata, StringComparison.OrdinalIgnoreCase)).ToList();

            if (udienzeFiltrate.Count == 0)
            {
                MessageBox.Show($"Nessuna udienza programmata per la classe {classeSelezionata}.", "Informazione", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = $"Salva Report Classe {classeSelezionata}",
                Filter = "File di Testo (*.txt)|*.txt|File CSV (*.csv)|*.csv",
                FileName = $"Report_Udienze_Classe_{classeSelezionata}.txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                EsportaUdienzeAFile(udienzeFiltrate, saveFileDialog.FileName, $"Report Classe - {classeSelezionata}");
            }
        }

        private void EsportaUdienzeAFile(List<UdienzaAssegnataDto> udienze, string path, string titolo)
        {
            try
            {
                string est = Path.GetExtension(path).ToLower();
                if (est == ".csv")
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine("Giorno;Ora;Professore;Classe;Aula;Piano");
                    foreach (var u in udienze.OrderBy(x => x.TurnoId))
                    {
                        var turno = CercaTurno(u.TurnoId);
                        string oraStr = turno != null ? $"{turno.OraInizio:hh\\:mm}-{turno.OraFine:hh\\:mm}" : "";
                        sb.AppendLine($"{u.TurnoGiorno};{oraStr};{u.NomeProfessore};{u.ClasseNome};{u.AulaNome};{u.AulaPiano}");
                    }
                    File.WriteAllText(path, sb.ToString(), System.Text.Encoding.UTF8);
                }
                else
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine("=================================================================================");
                    sb.AppendLine($"   {titolo.ToUpper()}   ");
                    sb.AppendLine("=================================================================================");
                    sb.AppendLine($"Numero Udienze Assegnate: {udienze.Count}");
                    sb.AppendLine("---------------------------------------------------------------------------------");
                    sb.AppendLine(string.Format("{0,-12} | {1,-12} | {2,-25} | {3,-10} | {4,-10}", "Giorno", "Orario", "Professore", "Classe", "Aula (Piano)"));
                    sb.AppendLine("---------------------------------------------------------------------------------");

                    foreach (var u in udienze.OrderBy(x => x.TurnoId))
                    {
                        var turno = CercaTurno(u.TurnoId);
                        string oraStr = turno != null ? $"{turno.OraInizio:hh\\:mm}-{turno.OraFine:hh\\:mm}" : "";
                        sb.AppendLine(string.Format("{0,-12} | {1,-12} | {2,-25} | {3,-10} | {4,-10}", 
                            u.TurnoGiorno, 
                            oraStr, 
                            u.NomeProfessore, 
                            u.ClasseNome, 
                            $"{u.AulaNome} (P{u.AulaPiano})"));
                    }
                    sb.AppendLine("=================================================================================");
                    File.WriteAllText(path, sb.ToString(), System.Text.Encoding.UTF8);
                }
                MessageBox.Show("Report esportato con successo!", "Esportazione Completata", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante l'esportazione: {ex.Message}", "Errore Esportazione", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}