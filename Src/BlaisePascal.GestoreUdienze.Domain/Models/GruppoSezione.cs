using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlaisePascal.GestoreUdienze.Domain.Models
{
    /// <summary>
    /// Gruppo di appartenenza di una classe in base alla sezione.
    /// Automazione = sezioni A, B, C, D  →  giornate 3 e 4
    /// Informatica  = sezioni E–N e BIO  →  giornate 1 e 2
    /// </summary>
    public enum GruppoSezione
    {
        Informatica,
        Automazione
    }
}

