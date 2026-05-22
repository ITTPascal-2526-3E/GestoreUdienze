using BlaisePascal.GestoreUdienze.Domain.Entities;
using System.Collections.Generic;

namespace BlaisePascal.GestoreUdienze.Domain.Interfaces
{
    public interface IAssegnazioneAuleService
    {
        IEnumerable<Udienza> Assegna(
            IEnumerable<Classe> classi,
            IEnumerable<Professore> professori,
            IEnumerable<Aula> auleDisponibili,
            IEnumerable<Turno> turni);
    }
}
