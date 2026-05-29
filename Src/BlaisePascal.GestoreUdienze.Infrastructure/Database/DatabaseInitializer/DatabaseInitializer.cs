using BlaisePascal.GestoreUdienze.Infrastructure.Database.Data;

namespace BlaisePascal.GestoreUdienze.Infrastructure.Database.DatabaseInitializer
{
    public static class DatabaseInitializer
    {
        public static void Initialize()
        {
            // FASE 1: CREAZIONE TABELLE (L'ordine non è vitale)
            ProfessoreRepository.CreaTabella();
            AulaRepository.CreaTabella();
            ClasseRepository.CreaTabella();
            MateriaRepository.CreaTabella();
            OrarioTurniRepository.CreaTabella();

            // FASE 2: SVUOTAMENTO TABELLE (ORDINE INVERSO! Prima i figli, poi il padre)
            OrarioTurniRepository.SvuotaTabella();
            MateriaRepository.SvuotaTabella();
            ClasseRepository.SvuotaTabella();
            AulaRepository.SvuotaTabella();
            ProfessoreRepository.SvuotaTabella(); // Il professore si svuota PER ULTIMO perché Classi e Materie hanno FK che puntano a Professori

            // FASE 3: SALVATAGGIO DATI (Usa "dati." invece di "DatiImportatiDto.")
            // TODO: Se un giorno i dati verranno salvati da un DTO unificato, inserire la logica qui
            // rispettando l'ordine corretto (prima Professori, poi Aule, poi Classi, Materie e OrariTurni).
            
        }
    }
}