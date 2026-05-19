using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace BlaisePascal.GestoreUdienze.Wpf
{
    public partial class MainWindow : Window
    {
        private string _caricatoFilePath = string.Empty;
        private string _caricatoPdfPath = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            AssegnaEventi();
            InizializzaInterfaccia();
        }

        private void InizializzaInterfaccia()
        {
            DatePickerGiornata.SelectedDate = DateTime.Now;

            // Carica subito i dati di prova nelle liste
            PopolaListeDaFile(string.Empty);

            // Attiva i controlli
            SetStatoControlli(true);
        }

        private void AssegnaEventi()
        {
            BtnBrowse.Click += BtnBrowse_Click;
            BtnBrowsePdf.Click += BtnBrowsePdf_Click; // Nuovo evento associato
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

        // 1. Sfoglia File Dati (Excel / CSV)
        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Seleziona il file dei dati delle udienze",
                Filter = "File Excel (*.xlsx;*.xls)|*.xlsx;*.xls|File CSV (*.csv)|*.csv",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _caricatoFilePath = openFileDialog.FileName;
                TxtFilePath.Text = Path.GetFileName(_caricatoFilePath);
                TxtFilePath.Foreground = System.Windows.Media.Brushes.Black;

                try
                {
                    PopolaListeDaFile(_caricatoFilePath);
                    SetStatoControlli(true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore durante la lettura del file dati: {ex.Message}", "Errore di Caricamento", MessageBoxButton.OK, MessageBoxImage.Error);
                    SetStatoControlli(false);
                }
            }
        }

        // 2. Sfoglia File PDF (Nuovo)
        private void BtnBrowsePdf_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Seleziona il file verbale PDF",
                Filter = "Documenti PDF (*.pdf)|*.pdf", // Accetta solo file PDF
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

        private void PopolaListeDaFile(string filePath)
        {
            List<string> auleGenerate = new List<string>();
            for (int i = 1; i <= 50; i++)
            {
                auleGenerate.Add($"Aula {i}");
            } 

            List<string> classiGenerate = new List<string>();

            string[] sezioni = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "L", "M", "N" };


            for (int anno = 1; anno <= 5; anno++)
            {

                foreach (string sezione in sezioni)
                {
                    classiGenerate.Add($"{anno}{sezione}");
                }

                classiGenerate.Add($"{anno}BIO");
            }
           
            ListAule.ItemsSource = auleGenerate;
            ListClassi.ItemsSource = classiGenerate;
        }

        private void BtnStampaGiornata_Click(object sender, RoutedEventArgs e)
        {
            DateTime? dataSelezionata = DatePickerGiornata.SelectedDate;
            if (!dataSelezionata.HasValue)
            {
                MessageBox.Show("Selezionare una data valida.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string dataString = dataSelezionata.Value.ToShortDateString();
            MessageBox.Show($"Generazione stampa per la giornata del: {dataString}", "Stampa Giornata", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnStampaAule_Click(object sender, RoutedEventArgs e)
        {
            List<string> auleSelezionate = new List<string>();
            foreach (var item in ListAule.Items)
            {
                if (ListAule.SelectedItems.Contains(item))
                {
                    auleSelezionate.Add(item.ToString());
                }
            }

            if (auleSelezionate.Count == 0)
            {
                MessageBox.Show("Selezionare almeno un'aula dalla lista.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show($"Generazione stampa per {auleSelezionate.Count} aule selezionate.", "Stampa Aule", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnStampaClassi_Click(object sender, RoutedEventArgs e)
        {
            if (ListClassi.SelectedItem == null)
            {
                MessageBox.Show("Selezionare una classe dalla lista.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string classeSelezionata = ListClassi.SelectedItem.ToString();
            MessageBox.Show($"Generazione stampa per la classe: {classeSelezionata}", "Stampa Classi", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void BtnGenera_Click(object sender, RoutedEventArgs e)
        {
            // 1. Controllo preliminare sul PDF
            if (string.IsNullOrEmpty(_caricatoPdfPath))
            {
                MessageBoxResult result = MessageBox.Show("Attenzione: nessun file PDF selezionato. Procedere comunque?", "Verifica Documento", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            // 2. Prepariamo l'interfaccia per il caricamento
            BtnGenera.IsEnabled = false;            // Disabilitiamo il pulsante per evitare click doppi
            PanelLoading.Visibility = Visibility.Visible; // Mostriamo la barra di caricamento
            ProgressBarGenera.Value = 0;            // Resettiamo il valore iniziale
            TxtPercentuale.Text = "0%";

            // 3. Simulazione del calcolo/avanzamento reale dell'elaborazione
            // (Puoi sostituire questo ciclo con i tuoi passaggi di esportazione reali)
            int stepTotali = 100;
            for (int i = 1; i <= stepTotali; i++)
            {
                // Ritardo asincrono artificiale per simulare il lavoro del computer (es. 30 millisecondi a step)
                await System.Threading.Tasks.Task.Delay(30);

                // Calcolo della percentuale matematica progressiva
                double percentuale = ((double)i / stepTotali) * 100;

                // Aggiorniamo i controlli grafici in tempo reale
                ProgressBarGenera.Value = percentuale;
                TxtPercentuale.Text = $"{(int)percentuale}%";
            }

            // 4. Elaborazione completata con successo
            MessageBox.Show("Elaborazione generale avviata con successo ed esportazione dei registri in corso.", "Elaborazione Generale", MessageBoxButton.OK, MessageBoxImage.Information);

            // 5. Ripristiniamo l'interfaccia allo stato iniziale
            PanelLoading.Visibility = Visibility.Collapsed; // Nascondiamo di nuovo la barra
            BtnGenera.IsEnabled = true;                    // Riabilitiamo il pulsante
        }

        private void TxtPdfPath_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void BtnBrowse_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}