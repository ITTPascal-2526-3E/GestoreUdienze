using BlaisePascal.GestoreUdienze.Domain.Models;
using System.Collections.Generic;

namespace BlaisePascal.GestoreUdienze.Domain.Algorithmic
{
    public interface IAssegnazioneAuleService
    {
        IEnumerable<AssegnazioneAula> Assegna(
            IEnumerable<Classe> classi,
            IEnumerable<Professore> professori,
            IEnumerable<Aula> auleDisponibili);
    }
}
