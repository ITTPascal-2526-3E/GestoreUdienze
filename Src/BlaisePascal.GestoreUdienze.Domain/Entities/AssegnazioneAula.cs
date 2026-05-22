namespace BlaisePascal.GestoreUdienze.Domain.Entities
{
    public class AssegnazioneAula
    {
        public Aula Aula { get; set; } = new Aula();
        public Professore Professore { get; set; } = new Professore();

        public AssegnazioneAula() { }

        public AssegnazioneAula(Aula aula, Professore professore)
        {
            Aula = aula;
            Professore = professore;
        }
    }
}
