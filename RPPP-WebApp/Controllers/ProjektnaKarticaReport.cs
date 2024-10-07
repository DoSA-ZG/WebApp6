using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PdfRpt.Core.Contracts;
using PdfRpt.FluentInterface;
using RPPP_WebApp.Data;
using RPPP_WebApp.Models;
using System.Text;

namespace RPPP_WebApp.Controllers
{
    public class ProjektnaKarticaReport : Controller
    {
        private readonly RPPP06Context _context;
        private readonly IWebHostEnvironment environment;
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public ProjektnaKarticaReport(RPPP06Context context, IWebHostEnvironment environment)
        {
            _context = context;
            this.environment = environment;
        }

        public async Task<IActionResult> ProjektnaKarticaMD2()
        {
            var kartica = await _context.ProjektnaKartica.AsNoTracking()
                .Include(p => p.Projekt)
                .Include(p => p.TransakcijaProjektnaKarticaIsporučitelj)
                .Include(p => p.TransakcijaProjektnaKarticaPrimatelj)
                .ToListAsync();

            byte[] content;

            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis projektnih kartica i transakcija";
                excel.Workbook.Properties.Author = "Filip Hlup";
                var worksheet = excel.Workbook.Worksheets.Add("ProjektneKartice");

                worksheet.Cells[1, 1].Value = "Banka";
                worksheet.Cells[1, 2].Value = "Iban";
                worksheet.Cells[1, 3].Value = "Stanje";
                worksheet.Cells[1, 4].Value = "Projekt";
                worksheet.Cells[1, 5].Value = "Ulazne transakcije";
                worksheet.Cells[1, 6].Value = "Izlazne transakcije";

                for (int i = 0; i < kartica.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = kartica[i].Banka;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment =
                        OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = kartica[i].Iban;
                    worksheet.Cells[i + 2, 3].Value = kartica[i].Stanje;
                    worksheet.Cells[i + 2, 4].Value = kartica[i].Projekt?.Ime;


                    List<Transakcija> ulazne = kartica[i].TransakcijaProjektnaKarticaPrimatelj.ToList();

                    StringBuilder builderic2 = new StringBuilder();

                    if (ulazne.Count == 0)
                    {
                        worksheet.Cells[i + 2, 5].Value = "-";
                    }
                    else
                    {
                        foreach (var VARIABLE in ulazne)
                        {
                            builderic2.Append($"Sa IBAN-a: {VARIABLE.IbanIsporučitelja} - Iznos: {VARIABLE.Iznos}€, ");
                        }

                        if (builderic2.ToString().Contains(", "))
                        {
                            builderic2.Remove(builderic2.Length - 2, 2);
                        }

                        worksheet.Cells[i + 2, 5].Value = builderic2;
                    }


                    List<Transakcija> izlazne = kartica[i].TransakcijaProjektnaKarticaIsporučitelj.ToList();

                    StringBuilder builderic = new StringBuilder();

                    if (izlazne.Count == 0)
                    {
                        worksheet.Cells[i + 2, 6].Value = "-";
                    }
                    else
                    {
                        foreach (var VARIABLE in izlazne)
                        {
                            builderic.Append($"Na IBAN: {VARIABLE.IbanPrimatelja} - Iznos: {VARIABLE.Iznos}€, ");
                        }

                        if (builderic.ToString().Contains(", "))
                        {
                            builderic.Remove(builderic.Length - 2, 2);
                        }

                        worksheet.Cells[i + 2, 6].Value = builderic;
                    }

                    worksheet.Cells[1, 1, kartica.Count + 1, 9].AutoFitColumns();
                }
                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "ProjektneKarticeMD.xlsx");
        }


        public async Task<IActionResult> ProjektneKartice()
        {
            var kartica = await _context.ProjektnaKartica.AsNoTracking()
                .Include(s => s.Projekt)
                .ToListAsync();

            byte[] content;

            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis projektnih kartica";
                excel.Workbook.Properties.Author = "Filip Hlup";
                var worksheet = excel.Workbook.Worksheets.Add("ProjektneKartice");

                worksheet.Cells[1, 1].Value = "Banka";
                worksheet.Cells[1, 2].Value = "Iban";
                worksheet.Cells[1, 3].Value = "Stanje";
                worksheet.Cells[1, 4].Value = "Projekt";

                for (int i = 0; i < kartica.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = kartica[i].Banka;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment =
                        OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = kartica[i].Iban;
                    worksheet.Cells[i + 2, 3].Value = kartica[i].Stanje;
                    worksheet.Cells[i + 2, 4].Value = kartica[i].Projekt?.Ime;

                }

                worksheet.Cells[1, 1, kartica.Count + 1, 9].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "ProjektneKartice.xlsx");
        }

        public async Task<IActionResult> Transakcije()
        {
            var transakcije = await _context.Transakcija.AsNoTracking()
                .Include(p => p.VrstaTransakcije)
                .Include(p => p.ProjektnaKarticaIsporučitelj)
                .Include(p => p.ProjektnaKarticaPrimatelj)
                .Include(p => p.ProjektnaKarticaIsporučitelj.Projekt)
                .Include(p => p.ProjektnaKarticaPrimatelj.Projekt)
                .ToListAsync();

            byte[] content;

            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis transakcija";
                excel.Workbook.Properties.Author = "Filip Hlup";
                var worksheet = excel.Workbook.Worksheets.Add("Transakcije");

                worksheet.Cells[1, 1].Value = "Iban isporučitelja";
                worksheet.Cells[1, 2].Value = "Iban primatelja";
                worksheet.Cells[1, 3].Value = "Iznos";
                worksheet.Cells[1, 4].Value = "Projektna kartica isporučitelj";
                worksheet.Cells[1, 5].Value = "Projektna kartica primatelj";
                worksheet.Cells[1, 6].Value = "Vrsta transakcije";

                for (int i = 0; i < transakcije.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = transakcije[i].IbanIsporučitelja;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment =
                        OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = transakcije[i].IbanPrimatelja;
                    worksheet.Cells[i + 2, 3].Value = transakcije[i].Iznos;
                    worksheet.Cells[i + 2, 4].Value = transakcije[i].ProjektnaKarticaIsporučiteljId;
                    worksheet.Cells[i + 2, 5].Value = transakcije[i].ProjektnaKarticaPrimateljId;
                    worksheet.Cells[i + 2, 6].Value = transakcije[i].VrstaTransakcije.Ime;
                }

                worksheet.Cells[1, 1, transakcije.Count + 1, 6].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "Transakcije.xlsx");
        }


        public async Task<IActionResult> ProjektneKarticePDF()
        {
            string naslov = "Popis projektnih kartica";
            var kartica = await _context.ProjektnaKartica.AsNoTracking()
                .Include(s => s.Projekt)
                .ToListAsync();

            PdfReport report = CreateReport(naslov);

            #region Podnožje i zaglavlje

            report.PagesFooter(footer => { footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy.")); })
                .PagesHeader(header =>
                {
                    header.CacheHeader(cache: true); // It's a default setting to improve the performance.
                    header.DefaultHeader(defaultHeader =>
                    {
                        defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
                        defaultHeader.Message(naslov);
                    });
                });

            #endregion

            #region Postavljanje izvora podataka i stupaca

            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(kartica));

            report.MainTableColumns(columns =>
            {
                columns.AddColumn(column =>
                {
                    columns.AddColumn(column =>
                    {
                        column.IsRowNumber(true);
                        column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                        column.IsVisible(true);
                        column.Order(0);
                        column.Width(1);
                        column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
                    });
                    column.PropertyName<ProjektnaKartica>(o => o.Banka);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(1);
                    column.HeaderCell("Banka", horizontalAlignment: HorizontalAlignment.Center);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<ProjektnaKartica>(o => o.Iban);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("IBAN", horizontalAlignment: HorizontalAlignment.Center);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<ProjektnaKartica>(o => o.Stanje);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(2);
                    column.HeaderCell("Stanje", horizontalAlignment: HorizontalAlignment.Center);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<ProjektnaKartica>(o => o.Projekt.Ime);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(2);
                    column.HeaderCell("Projekt", horizontalAlignment: HorizontalAlignment.Center);
                });
            });
            #endregion

            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=ProjektneKartice.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }


        public async Task<IActionResult> TransakcijePDF()
        {
            string naslov = "Popis transakcija";

            var transakcije = await _context.Transakcija.AsNoTracking()
                .Include(p => p.ProjektnaKarticaPrimatelj)
                .Include(p => p.ProjektnaKarticaIsporučitelj)
                .Include(p => p.VrstaTransakcije)
                .ToListAsync();

            PdfReport report = CreateReport(naslov);

            #region Podnožje i zaglavlje

            report.PagesFooter(footer => { footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy.")); })
                .PagesHeader(header =>
                {
                    header.CacheHeader(cache: true); // It's a default setting to improve the performance.
                    header.DefaultHeader(defaultHeader =>
                    {
                        defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
                        defaultHeader.Message(naslov);
                    });
                });

            #endregion

            #region Postavljanje izvora podataka i stupaca

            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(transakcije));

            report.MainTableColumns(columns =>
            {
                columns.AddColumn(column =>
                {
                    column.IsRowNumber(true);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(1);
                    column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Transakcija>(o => o.IbanIsporučitelja);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(2);
                    column.HeaderCell("IBAN Isporučitelja", horizontalAlignment: HorizontalAlignment.Center);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Transakcija>(o => o.IbanPrimatelja);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("IBAN Primatelja", horizontalAlignment: HorizontalAlignment.Center);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Transakcija>(o => o.Iznos);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(2);
                    column.HeaderCell("Iznos", horizontalAlignment: HorizontalAlignment.Center);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Transakcija>(o => o.ProjektnaKarticaIsporučiteljId);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(2);
                    column.HeaderCell("Projektna Kartica Isporučitelja", horizontalAlignment: HorizontalAlignment.Center);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Transakcija>(o => o.ProjektnaKarticaPrimateljId);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4);
                    column.Width(2);
                    column.HeaderCell("Projektna Kartica Primatelja", horizontalAlignment: HorizontalAlignment.Center);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Transakcija>(o => o.VrstaTransakcije.Ime);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4);
                    column.Width(2);
                    column.HeaderCell("Vrsta Transakcije", horizontalAlignment: HorizontalAlignment.Center);
                });
            });
            #endregion

            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=Transakcije.pdf");
                return File(pdf, "application/pdf");
                //return File(pdf, "application/pdf", "drzave.pdf"); //Otvara save as dialog
            }
            else
            {
                return NotFound();
            }
        }

        private PdfReport CreateReport(string naslov)
        {
            var pdf = new PdfReport();

            pdf.DocumentPreferences(doc =>
            {
                doc.Orientation(PageOrientation.Portrait);
                doc.PageSize(PdfPageSize.A4);
                doc.DocumentMetadata(new DocumentMetadata
                {
                    Author = "Filip Hlup",
                    Application = "RPPP",
                    Title = naslov
                });
                doc.Compression(new CompressionSettings
                {
                    EnableCompression = true,
                    EnableFullCompression = true
                });
            })
                //fix za linux https://github.com/VahidN/PdfReport.Core/issues/40
                .DefaultFonts(fonts =>
                {
                    fonts.Path(Path.Combine(environment.WebRootPath, "fonts", "verdana.ttf"),
                        Path.Combine(environment.WebRootPath, "fonts", "tahoma.ttf"));
                    fonts.Size(9);
                    fonts.Color(System.Drawing.Color.Black);
                })
                //
                .MainTableTemplate(template => { template.BasicTemplate(BasicTemplate.ProfessionalTemplate); })
                .MainTablePreferences(table =>
                {
                    table.ColumnsWidthsType(TableColumnWidthType.Relative);
                    //table.NumberOfDataRowsPerPage(20);
                    table.GroupsPreferences(new GroupsPreferences
                    {
                        GroupType = GroupType.HideGroupingColumns,
                        RepeatHeaderRowPerGroup = true,
                        ShowOneGroupPerPage = true,
                        SpacingBeforeAllGroupsSummary = 5f,
                        NewGroupAvailableSpacingThreshold = 150,
                        SpacingAfterAllGroupsSummary = 5f
                    });
                    table.SpacingAfter(4f);
                });

            return pdf;
        }


        [HttpPost]
        public async Task<IActionResult> UveziProjektnuKarticu(IFormFile excelFile)
        {
            List<ProjektnaKartica> karticeNove = new List<ProjektnaKartica>();
            List<ProjektnaKartica> uspjeh = new List<ProjektnaKartica>();
            List<ProjektnaKartica> neuspjeh = new List<ProjektnaKartica>();
            List<String> broj = new List<String>();

            if (excelFile == null || excelFile.Length == 0)
            {

                ModelState.AddModelError("excelFile", "Odaberite Excel datoteku.");
                return RedirectToAction("UvozIzvoz", "ProjektnaKartica");
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    await excelFile.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                        int rowStart = 2;
                        int rowCount = worksheet.Dimension.Rows;

                        for (int row = rowStart; row <= rowCount; row++)
                        {


                            var Banka = worksheet.Cells[row, 1].Value;
                            var Iban = worksheet.Cells[row, 2].Value;
                            var Stanje = worksheet.Cells[row, 3].Value;
                            var ProjektIme = worksheet.Cells[row, 4].Value;
                            if (ProjektIme != null)
                            {
                                ProjektIme = ProjektIme.ToString();
                            }

                            Projekt newProjekt = _context.Projekt.Where(p => p.Ime == ProjektIme).FirstOrDefault();


                            if (Banka != null && Iban != null && Stanje != null && newProjekt != null)
                            {
                                ProjektnaKartica kartica = new ProjektnaKartica
                                {
                                    Banka = Banka.ToString(),
                                    Iban = int.Parse(Iban.ToString()),
                                    Stanje = double.Parse(Stanje.ToString()),
                                    Projekt = newProjekt
                                };

                                var postoji = _context.ProjektnaKartica.Where(s => s.ProjektnaKarticaId == kartica.ProjektnaKarticaId).FirstOrDefault();
                                if (postoji != null)
                                {
                                    broj.Add("Nije dodan");
                                }
                                else
                                {
                                    karticeNove.Add(kartica);
                                    broj.Add("Dodan");
                                }
                            }
                            else
                            {
                                broj.Add("Nije dodan");
                            }
                        }
                    }
                }
                foreach (var VARIABLE in karticeNove)
                {

                    _context.Add(VARIABLE);
                    uspjeh.Add(VARIABLE);

                }
                await _context.SaveChangesAsync();

                if (uspjeh.Count == broj.Count)
                {
                    var stream = new MemoryStream();
                    await excelFile.CopyToAsync(stream);
                    var package = new ExcelPackage(stream);
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                    for (int i = 2; i <= worksheet.Dimension.Rows; i++)
                    {
                        worksheet.Cells[i, worksheet.Dimension.Columns + 1].Value = "DODAN";
                    }
                    byte[] updatedContent = package.GetAsByteArray();
                    Response.Headers.Add("content-disposition", "inline; filename=updatedFile.xlsx");
                    return File(updatedContent, ExcelContentType);
                }
                else
                {
                    int counte2r = 2;
                    var stream = new MemoryStream();
                    await excelFile.CopyToAsync(stream);
                    var package = new ExcelPackage(stream);
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int brojkol = worksheet.Dimension.Columns + 1;
                    foreach (var VARIABLE in broj)
                    {

                        worksheet.Cells[counte2r, brojkol].Value = VARIABLE;
                        counte2r = counte2r + 1;
                    }

                    byte[] updatedContent = package.GetAsByteArray();
                    Response.Headers.Add("content-disposition", "inline; filename=updatedFile.xlsx");
                    return File(updatedContent, ExcelContentType);
                }

            }
            catch (Exception ex)
            {
                TempData["errorMessage"] = $"Dogodila se greška!";
                return RedirectToAction("UvozIzvoz", "ProjektnaKartica");
            }


        }


        [HttpPost]
        public async Task<IActionResult> UveziTransakciju(IFormFile excelFile2)
        {
            List<Transakcija> transakcijeNove = new List<Transakcija>();
            List<Transakcija> uspjeh = new List<Transakcija>();
            List<Transakcija> neuspjeh = new List<Transakcija>();
            List<String> broj = new List<String>();

            if (excelFile2 == null || excelFile2.Length == 0)
            {

                ModelState.AddModelError("excelFile", "Odaberite Excel datoteku.");
                return RedirectToAction("UvozIzvoz", "ProjektnaKartica");
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    await excelFile2.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                        int rowStart = 2;
                        int rowCount = worksheet.Dimension.Rows;

                        for (int row = rowStart; row <= rowCount; row++)
                        {
                            ProjektnaKartica projektnaKarticaIsporučitelj = null;
                            ProjektnaKartica projektnaKarticaPrimatelj = null;

                            var IbanIsporučitelja = worksheet.Cells[row, 1].Value;
                            if (IbanIsporučitelja != null)
                            {
                                IbanIsporučitelja = int.Parse(IbanIsporučitelja.ToString());
                            }
                            var IbanPrimatelja = worksheet.Cells[row, 2].Value;
                            if (IbanPrimatelja != null)
                            {
                                IbanPrimatelja = int.Parse(IbanPrimatelja.ToString());
                            }
                            var Iznos = worksheet.Cells[row, 3].Value;
                            if (Iznos != null)
                            {
                                Iznos = double.Parse(Iznos.ToString());
                            }
                            var ProjektnaKarticaIsporučiteljId = worksheet.Cells[row, 4].Value;
                            if (ProjektnaKarticaIsporučiteljId != null)
                            {
                                ProjektnaKarticaIsporučiteljId = int.Parse(ProjektnaKarticaIsporučiteljId.ToString());
                                projektnaKarticaIsporučitelj = _context.ProjektnaKartica.Where(p => p.ProjektnaKarticaId == (int)ProjektnaKarticaIsporučiteljId).FirstOrDefault();
                            }
                            var ProjektnaKarticaPrimatelj = worksheet.Cells[row, 5].Value;
                            if (ProjektnaKarticaPrimatelj != null)
                            {
                                ProjektnaKarticaPrimatelj = int.Parse(ProjektnaKarticaPrimatelj.ToString());
                                projektnaKarticaPrimatelj = _context.ProjektnaKartica.Where(p => p.ProjektnaKarticaId == (int)ProjektnaKarticaPrimatelj).FirstOrDefault();
                            }
                            var VrstaTransakcije = worksheet.Cells[row, 6].Value;
                            if (VrstaTransakcije != null)
                            {
                                VrstaTransakcije = VrstaTransakcije.ToString();
                            }

                            VrstaTransakcije newVrstaTransakcije = _context.VrstaTransakcije.Where(v => v.Ime == (string) VrstaTransakcije).FirstOrDefault();

                            if (IbanIsporučitelja != null && !(IbanIsporučitelja.ToString().Length > 8) && IbanPrimatelja != null 
                                && !(IbanPrimatelja.ToString().Length > 8) && Iznos != null && VrstaTransakcije != null 
                                && (projektnaKarticaIsporučitelj != null || projektnaKarticaPrimatelj != null))
                            {

                                Transakcija transakcija = new Transakcija
                                {
                                    IbanIsporučitelja = (int) IbanIsporučitelja,
                                    IbanPrimatelja = (int) IbanPrimatelja,
                                    Iznos = (double) Iznos,
                                    ProjektnaKarticaIsporučitelj = projektnaKarticaIsporučitelj,
                                    ProjektnaKarticaPrimatelj = projektnaKarticaPrimatelj,
                                    VrstaTransakcije = newVrstaTransakcije
                                };
                                transakcijeNove.Add(transakcija);
                                broj.Add("Dodan");
                            }
                            else
                            {
                                broj.Add("Nije dodan");
                            }

                        }
                    }
                }

                foreach (var VARIABLE in transakcijeNove)
                {

                    _context.Add(VARIABLE);
                    uspjeh.Add(VARIABLE);

                }
                await _context.SaveChangesAsync();
                if (uspjeh.Count == broj.Count)
                {
                    var stream = new MemoryStream();
                    await excelFile2.CopyToAsync(stream);
                    var package = new ExcelPackage(stream);
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int brojkol = worksheet.Dimension.Columns + 1;
                    for (int i = 2; i <= worksheet.Dimension.Rows; i++)
                    {
                        worksheet.Cells[i, brojkol].Value = "DODAN";
                    }
                    byte[] updatedContent = package.GetAsByteArray();
                    Response.Headers.Add("content-disposition", "inline; filename=updatedFile.xlsx");
                    return File(updatedContent, ExcelContentType);
                }
                else
                {
                    int counte2r = 2;
                    var stream = new MemoryStream();
                    await excelFile2.CopyToAsync(stream);
                    var package = new ExcelPackage(stream);
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int brojkol = worksheet.Dimension.Columns + 1;
                    foreach (var VARIABLE in broj)
                    {

                        worksheet.Cells[counte2r, brojkol].Value = VARIABLE;
                        counte2r = counte2r + 1;
                    }

                    byte[] updatedContent = package.GetAsByteArray();
                    Response.Headers.Add("content-disposition", "inline; filename=updatedFile.xlsx");
                    return File(updatedContent, ExcelContentType);
                }
            }
            catch (Exception e)
            {
                TempData["errorMessage"] = $"Dogodila se greška!";
                return RedirectToAction("UvozIzvoz", "ProjektnaKartica");
            }
        }



    }

}