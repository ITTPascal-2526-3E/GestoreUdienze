using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlaisePascal.GestoreUdienze.Domain.Models
{
    /// <summary>
    /// Estende il modello Classe esistente aggiungendo
    /// la logica di gruppo (Automazione / Informatica) 
    /// e il parsing da stringa (es. "3L", "1BIO").
    /// </summary>
    public class Classe
    {
        private static readonly HashSet<string> SezioniAutomazione =
            new(StringComparer.OrdinalIgnoreCase) { "A", "B", "C", "D" };

        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;      // es. "3L"
        public string CodiceProfessore { get; set; } = string.Empty;

        // Lista di ID docenti che insegnano in questa classe
        public List<string> DocentiIds { get; set; } = new();

        // Campi derivati dal Nome
        public int Anno { get; private set; }
        public string Sezione { get; private set; } = string.Empty;

        public GruppoSezione Gruppo =>
            SezioniAutomazione.Contains(Sezione)
                ? GruppoSezione.Automazione
                : GruppoSezione.Informatica;

        public Classe() { }

        public Classe(int id, string nome, string codiceProfessore)
        {
            Id = id;
            CodiceProfessore = codiceProfessore;
            ImpostaNome(nome);
        }

        /// <summary>
        /// Imposta Nome e parsa Anno/Sezione dalla stringa (es. "3L", "1BIO").
        /// </summary>
        public void ImpostaNome(string nome)
        {
            nome = nome.Trim();
            Nome = nome;
            if (!string.IsNullOrEmpty(nome) && char.IsDigit(nome[0]))
            {
                Anno = int.Parse(nome[0].ToString());
                Sezione = nome[1..].ToUpperInvariant();
            }
        }

        /// <summary>
        /// Parsa una stringa come "3L" o "1BIO" in una Classe.
        /// </summary>
        public static Classe Parse(string raw, int id = 0)
        {
            var c = new Classe();
            c.Id = id;
            c.ImpostaNome(raw.Trim());
            return c;
        }

        public override string ToString() => Nome;
        public override bool Equals(object? obj) => obj is Classe c && Nome == c.Nome;
        public override int GetHashCode() => Nome.GetHashCode();
    }
}
