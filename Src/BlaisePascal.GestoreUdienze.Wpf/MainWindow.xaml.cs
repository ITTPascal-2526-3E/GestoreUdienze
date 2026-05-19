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

                // Opzionale: notifica visiva o logica aggiuntiva legata al PDF caricato
            }
        }

        private void PopolaListeDaFile(string filePath)
        {
            // -----------------------------------------
            // 1. GENERAZIONE AUTOMATICA DELLE 50 AULE
            // -----------------------------------------
            List<string> auleGenerate = new List<string>();
            for (int i = 1; i <= 50; i++)
            {
                auleGenerate.Add($"Aula {i}");
            }

            // -----------------------------------------
            // 2. GENERAZIONE AUTOMATICA DELLE CLASSI
            // -----------------------------------------
            List<string> classiGenerate = new List<string>();

            // Array delle sezioni dalla A alla N (senza lettere straniere J, K, W, X, Y)
            string[] sezioni = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "L", "M", "N" };

            // Ciclo per gli anni dalla 1° alla 5° classe
            for (int anno = 1; anno <= 5; anno++)
            {
                // Genera le combinazioni standard (es. 1A, 1B ... 5N)
                foreach (string sezione in sezioni)
                {
                    classiGenerate.Add($"{anno}{sezione}");
                }

                // Aggiunge la sezione speciale "BIO" per ogni anno (es. 1BIO, 2BIO ... 5BIO)
                classiGenerate.Add($"{anno}BIO");
            }

            // -----------------------------------------
            // 3. ASSEGNAZIONE DEI DATI AI CONTROLLI XAML
            // -----------------------------------------
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

        private void BtnGenera_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_caricatoPdfPath))
            {
                MessageBox.Show("Attenzione: nessun file PDF selezionato. Procedere comunque?", "Verifica Documento", MessageBoxButton.YesNo, MessageBoxImage.Question);
            }

            MessageBox.Show("Elaborazione generale avviata con successo ed esportazione dei registri in corso.", "Elaborazione Generale", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void TxtPdfPath_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void BtnBrowse_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}