using System.Collections.Generic;
using System.Windows;

namespace BlaisePascal.GestoreUdienze.Wpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            PopolaDati();
        }

        private void PopolaDati()
        {
            // 1. Popola Aule (saranno visualizzate con CheckBox grazie al Template XAML)
            List<string> aule = new List<string>();
            for (int i = 1; i <= 50; i++)
            {
                aule.Add($"Aula {i}");
            }
            ListAule.ItemsSource = aule;

            // 2. Popola Classi (Dalla 1A alla 5N + BIO)
            List<string> classi = new List<string>();
            char[] sezioni = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N' };

            for (int anno = 1; anno <= 5; anno++)
            {
                foreach (char sezione in sezioni)
                {
                    classi.Add($"{anno}{sezione}");
                }
                classi.Add($"{anno}BIO");
            }
            ListClassi.ItemsSource = classi;

            // 3. ListGiornata rimane vuoto per ora (da logica esterna)
        }
    }
}