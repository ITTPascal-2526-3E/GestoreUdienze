namespace BlaisePascal.GestoreUdienze.Domain.Entities
{
    
    public class Udienza
    {
        public Professore Docente { get; set; } = new Professore();
        public Classe Classe { get; set; } = new Classe();
        public Turno Turno { get; set; } = new Turno();
        public Aula Aula { get; set; } = new Aula();

        public Udienza() { }

        public Udienza(Professore docente, Classe classe, Turno turno, Aula aula)
        {
            Docente = docente;
            Classe = classe;
            Turno = turno;
            Aula = aula;
        }
    }
}
