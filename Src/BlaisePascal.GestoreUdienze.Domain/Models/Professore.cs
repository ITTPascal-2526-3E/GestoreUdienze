using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlaisePascal.GestoreUdienze.Domain.Models
{
    using System.Collections.Generic;

    public class Professore
    {
        public string CodiceProfessore { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Cognome { get; set; } = string.Empty;

        /// <summary>
        /// Se true, il professore è esente dal vincolo di conflitto per turno.
        /// Es. Baronio, Castagnoli, Cruciano.
        /// </summary>
        public bool IsEsente { get; set; } = false;

        /// <summary>
        /// Se true, il professore è attivo e deve essere programmato.
        /// </summary>
        public bool Attivo { get; set; } = true;

        /// <summary>
        /// Se true, il docente è di laboratorio (non ha vincoli di piano).
        /// </summary>
        public bool IsLaboratorio { get; set; } = false;

        /// <summary>
        /// Classi in cui il professore insegna.
        /// </summary>
        public List<Classe> Classi { get; set; } = new();

        public string NomeCompleto => $"{Cognome} {Nome}".Trim();

        public Professore() { }

        public Professore(string codiceProfessore, string nome, string cognome, bool isEsente = false, bool attivo = true, bool isLaboratorio = false)
        {
            CodiceProfessore = codiceProfessore;
            Nome = nome;
            Cognome = cognome;
            IsEsente = isEsente;
            Attivo = attivo;
            IsLaboratorio = isLaboratorio;
        }

        public override string ToString() => NomeCompleto;
    }
}
