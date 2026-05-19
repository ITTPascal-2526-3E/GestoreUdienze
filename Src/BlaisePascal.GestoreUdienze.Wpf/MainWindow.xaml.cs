using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace BlaisePascal.GestoreUdienze.Wpf
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Variabile per memorizzare il percorso del file di dati caricato (es. Excel, CSV, JSON)
        private string _caricatoFilePath = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            AssegnaEventi();
            InizializzaInterfaccia();
        }

        /// <summary>
        /// Inizializza i componenti dell'interfaccia grafica prima del caricamento del file.
        /// </summary>
        private void InizializzaInterfaccia()
        {
            DatePickerGiornata.SelectedDate = DateTime.Now;

            // Disabilita i controlli finché non viene caricato un file valido
            SetStatoControlli(false);
        }

        /// <summary>
        /// Sottoscrizione degli eventi dei controlli XAML.
        /// </summary>
        private void AssegnaEventi()
        {
            BtnBrowse.Click += BtnBrowse_Click;
            BtnStampaGiornata.Click += BtnStampaGiornata_Click;
            BtnStampaAule.Click += BtnStampaAule_Click;
            BtnStampaClassi.Click += BtnStampaClassi_Click;
            BtnGenera.Click += BtnGenera_Click;
        }

        /// <summary>
        /// Gestisce l'abilitazione dei pulsanti di stampa e generazione in base alla presenza del file.
        /// </summary>
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

        #region Logica di Caricamento Dati

        /// <summary>
        /// Evento per la selezione del file sorgente tramite OpenFileDialog.
        /// </summary>
        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Seleziona il file dei dati delle udienze",
                Filter = "File Excel (*.xlsx;*.xls)|*.xlsx;*.xls|File CSV (*.csv)|*.csv|Tutti i file (*.*)|*.*",
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
                    // Metodo per estrarre i dati dal file e popolare le liste
                    PopolaListeDaFile(_caricatoFilePath);
                    SetStatoControlli(true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore durante la lettura del file: {ex.Message}", "Errore di Caricamento", MessageBoxButton.OK, MessageBoxImage.Error);
                    SetStatoControlli(false);
                }
            }
        }

        /// <summary>
        /// Legge il file e popola i ListBox delle Aule e delle Classi.
        /// </summary>
        private void PopolaListeDaFile(string filePath)
        {
            // TODO: Inserire qui la logica reale di parsing del file (es. tramite Librerie Excel come EPPlus o ClosedXML)

            // Simulazione dati di esempio:
            List<string> auleEsempio = new List<string> { "Aula 1", "Aula 2", "Aula 3", "Aula Magna", "Laboratorio Info" };
            List<string> classiEsempio = new List<string> { "1A", "2A", "3B", "4C", "5B" };

            // Assegnazione dei dati alle liste dell'interfaccia
            ListAule.ItemsSource = auleEsempio;
            ListClassi.ItemsSource = classiEsempio;
        }

        #endregion

        #region Eventi di Stampa e Generazione

        /// <summary>
        /// Stampa il prospetto delle udienze relativo alla data selezionata.
        /// </summary>
        private void BtnStampaGiornata_Click(object sender, RoutedEventArgs e)
        {
            DateTime? dataSelezionata = DatePickerGiornata.SelectedDate;
            if (!dataSelezionata.HasValue)
            {
                MessageBox.Show("Selezionare una data valida.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string dataString = dataSelezionata.Value.ToShortDateString();

            // TODO: Implementare la logica di esportazione/stampa per la giornata
            MessageBox.Show($"Generazione stampa per la giornata del: {dataString}", "Stampa Giornata", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Stampa il prospetto relativo alle aule selezionate tramite CheckBox.
        /// </summary>
        private void BtnStampaAule_Click(object sender, RoutedEventArgs e)
        {
            List<string> auleSelezionate = new List<string>();

            // Recupera gli elementi selezionati dal ListBox delle Aule
            foreach (var item in ListAule.Items)
            {
                // Avendo abilitato la selezione multipla ed elementi con CheckBox, 
                // è possibile verificare quali elementi sono stati spuntati o selezionati
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

            // TODO: Implementare la logica di esportazione/stampa per le aule
            MessageBox.Show($"Generazione stampa per {auleSelezionate.Count} aule selezionate.", "Stampa Aule", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Stampa il prospetto relativo alla classe selezionata nel ListBox.
        /// </summary>
        private void BtnStampaClassi_Click(object sender, RoutedEventArgs e)
        {
            if (ListClassi.SelectedItem == null)
            {
                MessageBox.Show("Selezionare una classe dalla lista.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string classeSelezionata = ListClassi.SelectedItem.ToString();

            // TODO: Implementare la logica di esportazione/stampa per la classe
            MessageBox.Show($"Generazione stampa per la classe: {classeSelezionata}", "Stampa Classi", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Esegue la generazione complessiva di tutto il sistema delle udienze.
        /// </summary>
        private void BtnGenera_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Inserire la logica di elaborazione massiva finale

            MessageBox.Show("Elaborazione generale avviata con successo ed esportazione dei registri in corso.", "Elaborazione Generale", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion
    }
}