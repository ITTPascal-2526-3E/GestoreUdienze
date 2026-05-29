using BlaisePascal.GestoreUdienze.Application.Scheduling.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlaisePascal.GestoreUdienze.Application.Scheduling
{
    public static class ScheduleMapper
    {
        /// <summary>
        /// Prende la lista di UdienzaAssegnataDto in output dal solver e i turni di input,
        /// generando il modello piatto ottimizzato per i report QuestPDF.
        /// </summary>
        public static List<UdienzaSpedibilePdf> MapToPdfModel(
            List<UdienzaAssegnataDto> udienzeAssegnate,
            List<TurnoDto> turniInput)
        {
            var output = new List<UdienzaSpedibilePdf>();

            foreach (var ua in udienzeAssegnate)
            {
                // Recupera il giorno direttamente dall'udienza assegnata per evitare disallineamenti
                string stringaOrario = ua.TurnoGiorno;

                // Formatta la stringa del piano basandosi sul valore numerico (es. 0 -> Piano Terra)
                string stringaPiano = ua.AulaPiano == 0 ? "Piano Terra" : $"{ua.AulaPiano}° Piano";

                // Deduce l'ala dell'istituto in base al nome dell'aula
                string alaDeduci = ua.AulaNome.Contains("LAB", StringComparison.OrdinalIgnoreCase) ? "Nord (Laboratori)" : "Sud";

                // Assegna una dicitura alla materia basandosi sul contesto dell'aula assegnata dal solver
                string materiaDeduci = ua.AulaNome.Contains("LAB", StringComparison.OrdinalIgnoreCase) ? "Laboratorio / Informatica" : "Lezione Teorica / Colloquio";

                output.Add(new UdienzaSpedibilePdf
                {
                    NomeDocente = ua.NomeProfessore,
                    ClasseAssegnata = ua.ClasseNome,
                    Materia = materiaDeduci,
                    Aula = ua.AulaNome,
                    Ala = alaDeduci,
                    Piano = stringaPiano,
                    TurnoId = ua.TurnoId.ToString(),
                    OrarioFascia = stringaOrario
                });
            }

            return output;
        }
    }

    /// <summary>
    /// Modello dati intermedio usato per popolare le tabelle dei PDF
    /// </summary>
    public class UdienzaSpedibilePdf
    {
        public string NomeDocente { get; set; } = string.Empty;
        public string ClasseAssegnata { get; set; } = string.Empty;
        public string Materia { get; set; } = string.Empty;
        public string Aula { get; set; } = string.Empty;
        public string Ala { get; set; } = string.Empty;
        public string Piano { get; set; } = string.Empty;
        public string TurnoId { get; set; } = string.Empty;
        public string OrarioFascia { get; set; } = string.Empty;
    }
}

namespace BlaisePascal.GestoreUdienze.Application.Scheduling
{
    public static class PdfGenerator
    {
        // 1. PDF PER AULA (Orientamento Orizzontale per cartelli porta)
        public static void GeneraPdfAule(List<UdienzaSpedibilePdf> lista, string path)
        {
            Document.Create(container =>
            {
                var gruppiAula = lista.GroupBy(p => new { p.Aula, p.Piano, p.Ala }).OrderBy(g => g.Key.Aula);
                foreach (var gruppo in gruppiAula)
                {
                    container.Page(page =>
                    {
                        page.Margin(1.5f, Unit.Centimetre);
                        page.Size(PageSizes.A4.Landscape());

                        page.Header().Row(row => {
                            row.RelativeItem().Column(c => {
                                c.Item().Text("POSTAZIONE RICEVIMENTO").FontSize(12).FontColor("#9E9E9E");
                                c.Item().Text($"AULA: {gruppo.Key.Aula}").FontSize(28).ExtraBold().FontColor("#2196F3");
                            });
                            row.RelativeItem().AlignRight().Column(c => {
                                c.Item().AlignRight().Text($"{gruppo.Key.Ala}").FontSize(16).Bold();
                                c.Item().AlignRight().Text($"{gruppo.Key.Piano}").FontSize(14).Italic();
                            });
                        });

                        page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                        {
                            table.ColumnsDefinition(columns => {
                                columns.RelativeColumn(3);
                                columns.ConstantColumn(60);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(3);
                            });
                            table.Header(h => {
                                h.Cell().Element(HeaderStyle).Text("Docente");
                                h.Cell().Element(HeaderStyle).AlignCenter().Text("Classe");
                                h.Cell().Element(HeaderStyle).Text("Tipologia Attività");
                                h.Cell().Element(HeaderStyle).AlignCenter().Text("Orario e Giorno");
                            });

                            foreach (var item in gruppo.OrderBy(g => g.NomeDocente))
                            {
                                table.Cell().Element(RowStyle).Text(item.NomeDocente).SemiBold();
                                table.Cell().Element(RowStyle).AlignCenter().Text(item.ClasseAssegnata);
                                table.Cell().Element(RowStyle).Text(item.Materia);
                                table.Cell().Element(RowStyle).AlignCenter().Text(item.OrarioFascia);
                            }
                        });
                        page.Footer().AlignCenter().Text("Si prega di rispettare la scansione oraria").FontSize(10).Italic();
                    });
                }
            }).GeneratePdf(path);
        }

        // 2. PDF PER CLASSE (Prospetto sintetico utile a studenti/famiglie)
        public static void GeneraPdfClassi(List<UdienzaSpedibilePdf> lista, string path)
        {
            Document.Create(container =>
            {
                var gruppiClasse = lista.GroupBy(p => p.ClasseAssegnata).OrderBy(g => g.Key);
                foreach (var gruppo in gruppiClasse)
                {
                    container.Page(page =>
                    {
                        page.Margin(1, Unit.Centimetre);
                        page.Size(PageSizes.A4.Landscape());

                        page.Header().Row(row => {
                            row.RelativeItem().Text($"PIANIFICAZIONE UDIENZE - CLASSE {gruppo.Key}").FontSize(20).ExtraBold();
                            row.RelativeItem().AlignRight().Text("Blaise Pascal").FontSize(10);
                        });

                        page.Content().PaddingVertical(0.5f, Unit.Centimetre).Table(table =>
                        {
                            table.ColumnsDefinition(columns => {
                                columns.RelativeColumn(3); columns.RelativeColumn(3);
                                columns.ConstantColumn(70); columns.ConstantColumn(85); columns.ConstantColumn(70);
                                columns.ConstantColumn(50); columns.RelativeColumn(3);
                            });
                            table.Header(h => {
                                h.Cell().Element(HeaderStyle).Text("Docente");
                                h.Cell().Element(HeaderStyle).Text("Contesto");
                                h.Cell().Element(HeaderStyle).AlignCenter().Text("Aula");
                                h.Cell().Element(HeaderStyle).AlignCenter().Text("Ala");
                                h.Cell().Element(HeaderStyle).AlignCenter().Text("Piano");
                                h.Cell().Element(HeaderStyle).AlignCenter().Text("Turno");
                                h.Cell().Element(HeaderStyle).AlignCenter().Text("Orario Assegnato");
                            });

                            foreach (var item in gruppo.OrderBy(x => x.NomeDocente))
                            {
                                table.Cell().Element(RowStyle).Text(item.NomeDocente);
                                table.Cell().Element(RowStyle).Text(item.Materia);
                                table.Cell().Element(RowStyle).AlignCenter().Text(item.Aula);
                                table.Cell().Element(RowStyle).AlignCenter().Text(item.Ala);
                                table.Cell().Element(RowStyle).AlignCenter().Text(item.Piano);
                                table.Cell().Element(RowStyle).AlignCenter().Text(item.TurnoId);
                                table.Cell().Element(RowStyle).AlignCenter().Text(item.OrarioFascia);
                            }
                        });
                    });
                }
            }).GeneratePdf(path);
        }

        // 3. PDF GENERALE DI ISTITUTO (Tabellone di controllo)
        public static void GeneraPdfGenerale(List<UdienzaSpedibilePdf> lista, string path)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(1, Unit.Centimetre);
                    page.Size(PageSizes.A4.Landscape());

                    page.Header().PaddingBottom(0.5f, Unit.Centimetre).AlignCenter()
                        .Text("QUADRO GENERALE ASSEGNAZIONE UDIENZE").FontSize(18).ExtraBold().Underline();

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns => {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.ConstantColumn(50);
                            columns.ConstantColumn(65);
                            columns.ConstantColumn(85);
                            columns.ConstantColumn(65);
                            columns.ConstantColumn(50);
                            columns.RelativeColumn(3);
                        });
                        table.Header(h => {
                            h.Cell().Element(HeaderStyle).Text("Docente");
                            h.Cell().Element(HeaderStyle).Text("Contesto");
                            h.Cell().Element(HeaderStyle).AlignCenter().Text("Classe");
                            h.Cell().Element(HeaderStyle).AlignCenter().Text("Aula");
                            h.Cell().Element(HeaderStyle).AlignCenter().Text("Ala");
                            h.Cell().Element(HeaderStyle).AlignCenter().Text("Piano");
                            h.Cell().Element(HeaderStyle).AlignCenter().Text("Turno");
                            h.Cell().Element(HeaderStyle).AlignCenter().Text("Orario Esteso");
                        });

                        foreach (var item in lista.OrderBy(p => p.NomeDocente))
                        {
                            table.Cell().Element(RowStyle).Text(item.NomeDocente);
                            table.Cell().Element(RowStyle).Text(item.Materia);
                            table.Cell().Element(RowStyle).AlignCenter().Text(item.ClasseAssegnata);
                            table.Cell().Element(RowStyle).AlignCenter().Text(item.Aula);
                            table.Cell().Element(RowStyle).AlignCenter().Text(item.Ala);
                            table.Cell().Element(RowStyle).AlignCenter().Text(item.Piano);
                            table.Cell().Element(RowStyle).AlignCenter().Text(item.TurnoId);
                            table.Cell().Element(RowStyle).AlignCenter().Text(item.OrarioFascia);
                        }
                    });

                    page.Footer().AlignRight().Text(x => {
                        x.Span("Pagina "); x.CurrentPageNumber(); x.Span(" di "); x.TotalPages();
                    });
                });
            }).GeneratePdf(path);
        }

        // RISOLTO: Metodi Helper riscritti usando stringhe HEX esplicite universali per QuestPDF
        static IContainer HeaderStyle(IContainer c)
        {
            return c.DefaultTextStyle(x => x.SemiBold().FontSize(10))
                    .PaddingVertical(5)
                    .BorderBottom(1)
                    .BorderColor("#000000"); // Nero Nativo esadecimale
        }

        static IContainer RowStyle(IContainer c)
        {
            return c.BorderBottom(1)
                    .BorderColor("#E0E0E0") // Grigio Chiaro Nativo esadecimale (pienamente supportato)
                    .PaddingVertical(8)
                    .DefaultTextStyle(x => x.FontSize(10));
        }
    }
}