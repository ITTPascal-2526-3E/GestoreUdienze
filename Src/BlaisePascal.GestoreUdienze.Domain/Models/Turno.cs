namespace BlaisePascal.GestoreUdienze.Domain.Models
{
    using System;

    public class Turno
    {
        public const int MaxClassi = 4;
        public const int MinClassi = 3;

        public int NumeroGiornata { get; set; }   // 1–4
        public int IndiceTurno { get; set; }      // 1–9 dentro la giornata

        /// <summary>Indice globale assoluto 1–36.</summary>
        public int IndiceGlobale => (NumeroGiornata - 1) * 9 + IndiceTurno;

        public GruppoSezione Gruppo =>
            NumeroGiornata <= 2 ? GruppoSezione.Informatica : GruppoSezione.Automazione;

        public List<Classe> Classi { get; set; } = new();

        // Hook Fase 2 — aule assegnate
        public List<AssegnazioneAula> AssegnazioniAule { get; set; } = new();

        public bool IsPieno => Classi.Count >= MaxClassi;
        public bool IsDisponibile => !IsPieno;
        public int NumeroClassi => Classi.Count;

        public Turno() { }

        public Turno(int numeroGiornata, int indiceTurno)
        {
            if (numeroGiornata < 1 || numeroGiornata > 4)
                throw new ArgumentOutOfRangeException(nameof(numeroGiornata));
            if (indiceTurno < 1 || indiceTurno > 9)
                throw new ArgumentOutOfRangeException(nameof(indiceTurno));

            NumeroGiornata = numeroGiornata;
            IndiceTurno = indiceTurno;
        }

        public void AggiungiClasse(Classe classe)
        {
            if (IsPieno)
                throw new InvalidOperationException(
                    $"Turno {NumeroGiornata}-{IndiceTurno} già pieno.");
            Classi.Add(classe);
        }

        public override string ToString() =>
            $"G{NumeroGiornata}-T{IndiceTurno} [{string.Join(", ", Classi)}]";

    }
}
