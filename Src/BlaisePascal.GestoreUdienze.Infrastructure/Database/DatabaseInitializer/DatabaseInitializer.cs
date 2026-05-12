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
            OrarioTurniRepository.CreaTabella();

            // FASE 2: SVUOTAMENTO TABELLE (ORDINE INVERSO! Prima i figli, poi il padre)
            AulaRepository.SvuotaTabella();
            OrarioTurniRepository.SvuotaTabella();
            ClasseRepository.SvuotaTabella();
            ProfessoreRepository.SvuotaTabella(); // Il professore si svuota PER ULTIMO perché Aule e OrarioTurni hanno FK che puntano a Professori

            // FASE 3: SALVATAGGIO DATI (Usa "dati." invece di "DatiImportatiDto.")
            // ORDINE CORRETTO: Prima il padre, poi i figli
            ProfessoreRepository.SalvaProfessori(DatiImportatiDto.Professori);
            AulaRepository.SalvaAule(DatiImportatiDto.Aule);
            ClasseRepository.SalvaClassi(DatiImportatiDto.Classi);
            OrarioTurniRepository.SalvaOrarioTurni(DatiImportatiDto.OrariTurni);
        }

    }
}