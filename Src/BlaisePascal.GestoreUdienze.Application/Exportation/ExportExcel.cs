using BlaisePascal.GestoreUdienze.Application.Scheduling.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlaisePascal.GestoreUdienze.Application.Reporting
{
    public class EsportazioneExcelService
    {
        public EsportazioneExcelService()
        {
            // Imposta la licenza per l'uso non commerciale
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }


        public async Task EseguiEsportazioniAsync(RisultatoSchedulingDto risultato, string classeDaFiltrare = "1I Informatica", string aulaDaFiltrare = "Aula 53")
        {
            if (risultato == null || !risultato.Udienze.Any())
            {
                Console.WriteLine("Nessun dato da esportare. Il solver non ha prodotto udienze.");
                return;
            }

            var task1 = StampaAulePerClasseAsync(risultato.Udienze, classeDaFiltrare);
            var task2 = TuttiDocentiAsync(risultato.Udienze);
            var task3 = DocentiNellAulaAsync(risultato.Udienze, aulaDaFiltrare);

            // Attende che TUTTI e tre i file abbiano finito di essere generati e salvati
            await Task.WhenAll(task1, task2, task3);
        }

        public async Task StampaAulePerClasseAsync(List<UdienzaAssegnataDto> udienze, string nomeClasse)
        {
            // Filtra le udienze per la classe specificata
            var udienzeClasse = udienze
                .Where(u => u.ClasseNome == nomeClasse)
                .OrderBy(s => s.NomeProfessore)
                .ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("AuleUdienze");

                worksheet.Cells[1, 1, 1, 5].Merge = true;
                worksheet.Cells[1, 1].Value = $"Udienze - Classe: {nomeClasse}";
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Intestazioni
                worksheet.Cells[3, 1].Value = "Docente";
                worksheet.Cells[3, 2].Value = "Aula";
                worksheet.Cells[3, 3].Value = "Piano";
                worksheet.Cells[3, 4].Value = "Giorno";
                worksheet.Cells[3, 5].Value = "Turno";

                using (var range = worksheet.Cells[1, 1, 3, 5])
                {
                    range.Style.Font.Bold = true;
                }

                int row = 4;
                foreach (var item in udienzeClasse)
                {
                    worksheet.Cells[row, 1].Value = item.NomeProfessore;
                    worksheet.Cells[row, 2].Value = item.AulaNome;
                    worksheet.Cells[row, 3].Value = item.AulaPiano;
                    worksheet.Cells[row, 4].Value = item.TurnoGiorno.ToString("dd/MM/yyyy"); // Adatta il formato data se necessario
                    worksheet.Cells[row, 5].Value = item.TurnoId;
                    row++;
                }

                worksheet.Cells.AutoFitColumns();

                ApplicaBordi(worksheet, 3, 1, row - 1, 5);

                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string percorso = Path.Combine(desktop, $"Udienze_Classe_{nomeClasse.Replace(" ", "_")}.xlsx");

                await package.SaveAsAsync(new FileInfo(percorso));
            }
        }

        public async Task TuttiDocentiAsync(List<UdienzaAssegnataDto> udienze)
        {
            var tutteLeUdienze = udienze.OrderBy(s => s.NomeProfessore).ThenBy(s => s.TurnoGiorno).ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("TutteUdienze");

                worksheet.Cells[1, 1, 1, 6].Merge = true;
                worksheet.Cells[1, 1].Value = "Quadro Generale Udienze";
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Intestazioni
                worksheet.Cells[3, 1].Value = "Docente";
                worksheet.Cells[3, 2].Value = "Classe";
                worksheet.Cells[3, 3].Value = "Aula";
                worksheet.Cells[3, 4].Value = "Piano";
                worksheet.Cells[3, 5].Value = "Giorno";
                worksheet.Cells[3, 6].Value = "Turno";

                using (var range = worksheet.Cells[1, 1, 3, 6])
                {
                    range.Style.Font.Bold = true;
                }

                int row = 4;
                foreach (var item in tutteLeUdienze)
                {
                    worksheet.Cells[row, 1].Value = item.NomeProfessore;
                    worksheet.Cells[row, 2].Value = item.ClasseNome;
                    worksheet.Cells[row, 3].Value = item.AulaNome;
                    worksheet.Cells[row, 4].Value = item.AulaPiano;
                    worksheet.Cells[row, 5].Value = item.TurnoGiorno.ToString("dd/MM/yyyy");
                    worksheet.Cells[row, 6].Value = item.TurnoId;
                    row++;
                }

                worksheet.Cells.AutoFitColumns();

                ApplicaBordi(worksheet, 3, 1, row - 1, 6);

                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string percorso = Path.Combine(desktop, "InformazioniGeneraliUdienze.xlsx");

                await package.SaveAsAsync(new FileInfo(percorso));
            }
        }

        public async Task DocentiNellAulaAsync(List<UdienzaAssegnataDto> udienze, string nomeAula)
        {
            var udienzeAula = udienze
                .Where(u => u.AulaNome == nomeAula)
                .OrderBy(s => s.TurnoGiorno)
                .ThenBy(s => s.TurnoId)
                .ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("DocentiInAula");

                worksheet.Cells[1, 1, 1, 4].Merge = true;
                worksheet.Cells[1, 1].Value = $"Udienze in {nomeAula}";
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Intestazioni
                worksheet.Cells[3, 1].Value = "Docente";
                worksheet.Cells[3, 2].Value = "Classe";
                worksheet.Cells[3, 3].Value = "Giorno";
                worksheet.Cells[3, 4].Value = "Turno";

                using (var range = worksheet.Cells[1, 1, 3, 4])
                {
                    range.Style.Font.Bold = true;
                }

                int row = 4;
                foreach (var item in udienzeAula)
                {
                    worksheet.Cells[row, 1].Value = item.NomeProfessore;
                    worksheet.Cells[row, 2].Value = item.ClasseNome;
                    worksheet.Cells[row, 3].Value = item.TurnoGiorno.ToString("dd/MM/yyyy");
                    worksheet.Cells[row, 4].Value = item.TurnoId;
                    row++;
                }

                worksheet.Cells.AutoFitColumns();

                ApplicaBordi(worksheet, 3, 1, row - 1, 4);

                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string percorso = Path.Combine(desktop, $"DocentiNellAula_{nomeAula.Replace(" ", "_")}.xlsx");

                await package.SaveAsAsync(new FileInfo(percorso));
            }
        }


        private void ApplicaBordi(ExcelWorksheet worksheet, int fromRow, int fromCol, int toRow, int toCol)
        {
            if (toRow < fromRow) return; // Evita eccezioni se non ci sono dati

            using (var range = worksheet.Cells[fromRow, fromCol, toRow, toCol])
            {
                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }
        }
    }
}