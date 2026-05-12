using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

namespace BlaisePascal.GestoreUdienze.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Percorsi dei file selezionati
        private string? _filePathDocenti;
        private string? _filePathAule;

        // Dati importati
        private List<string[]>? _datiDocenti;
        private List<string[]>? _datiAule;

        // Flag di importazione completata
        private bool _docentiImportati;
        private bool _auleImportate;

        public MainWindow()
        {
            InitializeComponent();
        }

        // =============================================
        //  STEP 1 – Importa Docenti
        // =============================================

        /// <summary>
        /// Apre un dialogo per selezionare il file CSV/TXT dei docenti.
        /// </summary>
        private void BtnBrowseDocenti_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Seleziona il file dei docenti",
                Filter = "File PDF (*.pdf)|*.pdf|Tutti i file (*.*)|*.*",
                DefaultExt = ".pdf"
            };

            if (dialog.ShowDialog() == true)
            {
                _filePathDocenti = dialog.FileName;
                TxtFilePathDocenti.Text = _filePathDocenti;
                BtnImportaDocenti.IsEnabled = true;

                // Reset stato precedente
                ResetStatusDocenti();
            }
        }

        /// <summary>
        /// Legge e importa i dati dal file docenti selezionato.
        /// </summary>
        private async void BtnImportaDocenti_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_filePathDocenti) || !File.Exists(_filePathDocenti))
            {
                MessageBox.Show("Il file selezionato non esiste o il percorso non è valido.",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Disabilita i pulsanti durante l'importazione
                BtnImportaDocenti.IsEnabled = false;
                BtnBrowseDocenti.IsEnabled = false;

                // Mostra progress
                ProgBarDocenti.Visibility = Visibility.Visible;
                ProgBarDocenti.Value = 0;
                TxtStatusDocenti.Visibility = Visibility.Visible;
                TxtStatusDocenti.Text = "Lettura file docenti in corso...";
                TxtStatusDocenti.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748b"));

                // Leggi il file in modo asincrono
                var lines = await Task.Run(() => File.ReadAllLines(_filePathDocenti, Encoding.UTF8));

                if (lines.Length == 0)
                {
                    MostraErroreDocenti("Il file è vuoto.");
                    return;
                }

                _datiDocenti = new List<string[]>();
                int totalLines = lines.Length;

                for (int i = 0; i < totalLines; i++)
                {
                    // Salta righe vuote
                    if (string.IsNullOrWhiteSpace(lines[i]))
                        continue;

                    // Parsing CSV (separatore ; o ,)
                    char separator = lines[i].Contains(';') ? ';' : ',';
                    var campos = lines[i].Split(separator);
                    _datiDocenti.Add(campos);

                    // Aggiorna progress bar
                    int percentuale = (int)((i + 1.0) / totalLines * 100);
                    ProgBarDocenti.Value = percentuale;
                    TxtStatusDocenti.Text = $"Importazione docenti... {percentuale}% ({i + 1}/{totalLines} righe)";

                    // Consenti aggiornamento UI
                    if (i % 50 == 0)
                        await Task.Delay(1);
                }

                ProgBarDocenti.Value = 100;
                _docentiImportati = true;

                TxtStatusDocenti.Text = $"✓ Importazione completata: {_datiDocenti.Count} docenti importati.";
                TxtStatusDocenti.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10b981"));

            }
            catch (Exception ex)
            {
                MostraErroreDocenti($"Errore durante l'importazione: {ex.Message}");
            }
            finally
            {
                BtnBrowseDocenti.IsEnabled = true;
                BtnImportaDocenti.IsEnabled = true;
            }
        }

        private void ResetStatusDocenti()
        {
            _docentiImportati = false;
            _datiDocenti = null;
            ProgBarDocenti.Visibility = Visibility.Collapsed;
            ProgBarDocenti.Value = 0;
            TxtStatusDocenti.Visibility = Visibility.Collapsed;
            TxtStatusDocenti.Text = "";
        }

        private void MostraErroreDocenti(string messaggio)
        {
            TxtStatusDocenti.Visibility = Visibility.Visible;
            TxtStatusDocenti.Text = $"✗ {messaggio}";
            TxtStatusDocenti.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ef4444"));
            BtnBrowseDocenti.IsEnabled = true;
            BtnImportaDocenti.IsEnabled = true;
        }

        // =============================================
        //  STEP 2 – Importa Aule
        // =============================================

        /// <summary>
        /// Apre un dialogo per selezionare il file CSV/TXT delle aule.
        /// </summary>
        private void BtnBrowseAule_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Seleziona il file delle aule",
                Filter = "File PDF (*.pdf)|*.pdf|Tutti i file (*.*)|*.*",
                DefaultExt = ".pdf"
            };

            if (dialog.ShowDialog() == true)
            {
                _filePathAule = dialog.FileName;
                TxtFilePathAule.Text = _filePathAule;
                BtnImportaAule.IsEnabled = true;

                // Reset stato precedente
                ResetStatusAule();
            }
        }

        /// <summary>
        /// Legge e importa i dati dal file aule selezionato.
        /// </summary>
        private async void BtnImportaAule_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_filePathAule) || !File.Exists(_filePathAule))
            {
                MessageBox.Show("Il file selezionato non esiste o il percorso non è valido.",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Disabilita i pulsanti durante l'importazione
                BtnImportaAule.IsEnabled = false;
                BtnBrowseAule.IsEnabled = false;

                // Mostra progress
                ProgBarAule.Visibility = Visibility.Visible;
                ProgBarAule.Value = 0;
                TxtStatusAule.Visibility = Visibility.Visible;
                TxtStatusAule.Text = "Lettura file aule in corso...";
                TxtStatusAule.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748b"));

                // Leggi il file in modo asincrono
                var lines = await Task.Run(() => File.ReadAllLines(_filePathAule, Encoding.UTF8));

                if (lines.Length == 0)
                {
                    MostraErroreAule("Il file è vuoto.");
                    return;
                }

                _datiAule = new List<string[]>();
                int totalLines = lines.Length;

                for (int i = 0; i < totalLines; i++)
                {
                    // Salta righe vuote
                    if (string.IsNullOrWhiteSpace(lines[i]))
                        continue;

                    // Parsing CSV (separatore ; o ,)
                    char separator = lines[i].Contains(';') ? ';' : ',';
                    var campos = lines[i].Split(separator);
                    _datiAule.Add(campos);

                    // Aggiorna progress bar
                    int percentuale = (int)((i + 1.0) / totalLines * 100);
                    ProgBarAule.Value = percentuale;
                    TxtStatusAule.Text = $"Importazione aule... {percentuale}% ({i + 1}/{totalLines} righe)";

                    // Consenti aggiornamento UI
                    if (i % 50 == 0)
                        await Task.Delay(1);
                }

                ProgBarAule.Value = 100;
                _auleImportate = true;

                TxtStatusAule.Text = $"✓ Importazione completata: {_datiAule.Count} aule importate.";
                TxtStatusAule.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10b981"));

            }
            catch (Exception ex)
            {
                MostraErroreAule($"Errore durante l'importazione: {ex.Message}");
            }
            finally
            {
                BtnBrowseAule.IsEnabled = true;
                BtnImportaAule.IsEnabled = true;
            }
        }

        private void ResetStatusAule()
        {
            _auleImportate = false;
            _datiAule = null;
            ProgBarAule.Visibility = Visibility.Collapsed;
            ProgBarAule.Value = 0;
            TxtStatusAule.Visibility = Visibility.Collapsed;
            TxtStatusAule.Text = "";
        }

        private void MostraErroreAule(string messaggio)
        {
            TxtStatusAule.Visibility = Visibility.Visible;
            TxtStatusAule.Text = $"✗ {messaggio}";
            TxtStatusAule.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ef4444"));
            BtnBrowseAule.IsEnabled = true;
            BtnImportaAule.IsEnabled = true;
        }

        // =============================================
        //  STEP 3 – Scarica Risultati
        // =============================================

        /// <summary>
        /// Esporta i dati importati in un file CSV.
        /// </summary>
        private async void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            if (!_docentiImportati && !_auleImportate)
            {
                MessageBox.Show("Nessun dato importato. Importa prima i docenti e/o le aule.",
                    "Attenzione", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Title = "Salva risultati",
                Filter = "File PDF (*.pdf)|*.pdf|File di testo (*.txt)|*.txt",
                DefaultExt = ".pdf",
                FileName = $"risultati_udienze_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (saveDialog.ShowDialog() != true)
                return;

            try
            {
                BtnDownload.IsEnabled = false;
                ProgBarGenerazione.Visibility = Visibility.Visible;
                TxtStatusGenerazione.Visibility = Visibility.Visible;
                TxtStatusGenerazione.Text = "Generazione file in corso...";

                await Task.Run(() =>
                {
                    var sb = new StringBuilder();

                    // Sezione Docenti
                    if (_datiDocenti != null && _datiDocenti.Count > 0)
                    {
                        sb.AppendLine("=== DOCENTI ===");
                        foreach (var riga in _datiDocenti)
                        {
                            sb.AppendLine(string.Join(";", riga));
                        }
                        sb.AppendLine();
                    }

                    // Sezione Aule
                    if (_datiAule != null && _datiAule.Count > 0)
                    {
                        sb.AppendLine("=== AULE ===");
                        foreach (var riga in _datiAule)
                        {
                            sb.AppendLine(string.Join(";", riga));
                        }
                    }

                    File.WriteAllText(saveDialog.FileName, sb.ToString(), Encoding.UTF8);
                });

                ProgBarGenerazione.Visibility = Visibility.Collapsed;
                TxtStatusGenerazione.Text = $"✓ File salvato con successo!";
                TxtStatusGenerazione.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10b981"));

                MessageBox.Show($"File salvato in:\n{saveDialog.FileName}",
                    "Esportazione completata", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ProgBarGenerazione.Visibility = Visibility.Collapsed;
                TxtStatusGenerazione.Text = $"✗ Errore: {ex.Message}";
                TxtStatusGenerazione.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ef4444"));

                MessageBox.Show($"Errore durante il salvataggio:\n{ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnDownload.IsEnabled = true;
            }
        }
    }
}