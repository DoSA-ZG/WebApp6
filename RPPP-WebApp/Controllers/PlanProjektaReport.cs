using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PdfRpt.FluentInterface;
using PdfRpt.Core.Contracts;
using RPPP_WebApp.Data;
using RPPP_WebApp.Models;
using System.Text;
using SkiaSharp;
using System.Numerics;

namespace RPPP_WebApp.Controllers
{
    public class PlanProjektaReport : Controller
    {
        private readonly RPPP06Context _context;
        private readonly IWebHostEnvironment environment;
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public PlanProjektaReport(RPPP06Context context, IWebHostEnvironment environment)
        {
            _context = context;
            this.environment = environment;
        }

        public async Task<IActionResult> PlanProjektaMD2()
        {
            var planovi = await _context.PlanProjekta.AsNoTracking()
                .Include(s => s.Projekt)
                .Include(p => p.Etapa)
                .Include(s => s.VoditeljEmailNavigation)
                .ToListAsync();

            byte[] content;

            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Izvještaj o planovima projekata te njihovim etapama";
                excel.Workbook.Properties.Author = "Daria Bevanda";

                foreach (PlanProjekta plan in planovi)
                {
                    var title = plan.PlanProjektaId.ToString();
                    var worksheet = excel.Workbook.Worksheets.Add(title);

                    worksheet.Cells[1, 1].Value = "ID";
                    worksheet.Cells[1, 2].Value = "Planirani početak";
                    worksheet.Cells[1, 3].Value = "Planirani kraj";
                    worksheet.Cells[1, 4].Value = "Stvarni početak";
                    worksheet.Cells[1, 5].Value = "Stvarni kraj";
                    worksheet.Cells[1, 6].Value = "Voditelj";
                    worksheet.Cells[1, 7].Value = "Projekt (kratica)";
                    worksheet.Cells[1, 8].Value = "Etape";

                    worksheet.Cells[2, 1].Value = plan.PlanProjektaId;
                    worksheet.Cells[2, 1].Style.HorizontalAlignment =
                        OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[2, 2].Value = plan.PlaniraniPočetak.ToString("dd-MM-yyyy HH:mm");
                    worksheet.Cells[2, 3].Value = plan.PlaniraniKraj.ToString("dd-MM-yyyy HH:mm");
                    worksheet.Cells[2, 4].Value = plan.StvarniPočetak?.ToString("dd-MM-yyyy HH:mm");
                    worksheet.Cells[2, 5].Value = plan.StvarniKraj?.ToString("dd-MM-yyyy HH:mm");
                    worksheet.Cells[2, 6].Value = plan.VoditeljEmailNavigation.Ime.ToString() + " " + plan.VoditeljEmailNavigation.Prezime.ToString();
                    worksheet.Cells[2, 7].Value = plan.Projekt.Kratica;

                    // Details
                    List<Etapa> etape = plan.Etapa.ToList();

                    StringBuilder builderEx = new StringBuilder();

                    if (etape.Count == 0)
                    {
                        worksheet.Cells[2, 8].Value = "-";
                    }
                    else
                    {
                        foreach (var e in etape)
                        {
                            builderEx.Append(e.Ime + ", ");
                        }

                        if (builderEx.ToString().Contains(", "))
                        {
                            builderEx.Remove(builderEx.Length - 2, 2);
                        }

                        worksheet.Cells[2, 8].Value = builderEx;
                    }

                    worksheet.Cells[1, 1, 1, 8].AutoFitColumns();
                }

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "PlanoviMD.xlsx");
        }

        public async Task<IActionResult> Planovi()
        {
            var planovi = await _context.PlanProjekta.AsNoTracking()
                .Include(s => s.Projekt)
                .Include(p => p.Etapa)
                .Include(s => s.VoditeljEmailNavigation)
                .ToListAsync();

            byte[] content;

            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Planovi";
                excel.Workbook.Properties.Author = "Daria Bevanda";
                var worksheet = excel.Workbook.Worksheets.Add("Planovi");

                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Planirani početak";
                worksheet.Cells[1, 3].Value = "Planirani kraj";
                worksheet.Cells[1, 4].Value = "Stvarni početak";
                worksheet.Cells[1, 5].Value = "Stvarni kraj";
                worksheet.Cells[1, 6].Value = "Voditelj";
                worksheet.Cells[1, 7].Value = "Projekt (kratica)";

                for (int i = 0; i < planovi.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = planovi[i].PlanProjektaId;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment =
                        OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = planovi[i].PlaniraniPočetak.ToString("dd-MM-yyyy HH:mm");
                    worksheet.Cells[i + 2, 3].Value = planovi[i].PlaniraniKraj.ToString("dd-MM-yyyy HH:mm");
                    worksheet.Cells[i + 2, 4].Value = planovi[i].StvarniPočetak?.ToString("dd-MM-yyyy HH:mm");
                    worksheet.Cells[i + 2, 5].Value = planovi[i].StvarniKraj?.ToString("dd-MM-yyyy HH:mm");
                    worksheet.Cells[i + 2, 6].Value = planovi[i].VoditeljEmailNavigation.Ime.ToString() + " " + planovi[i].VoditeljEmailNavigation.Prezime.ToString();
                    worksheet.Cells[i + 2, 7].Value = planovi[i].Projekt.Kratica;
                }

                worksheet.Cells[1, 1, planovi.Count + 1, 7].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "Planovi.xlsx");
        }

        public async Task<IActionResult> Etape()
        {
            var etape = await _context.Etapa.AsNoTracking()
                .Include(p => p.PlanProjekta.Projekt)
                .Include(p => p.Aktivnost)
                .Include(p => p.PlanProjekta)
                .ToListAsync();

            byte[] content;

            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Etape";
                excel.Workbook.Properties.Author = "Daria Bevanda";
                var worksheet = excel.Workbook.Worksheets.Add("Etape");

                worksheet.Cells[1, 1].Value = "Ime";
                worksheet.Cells[1, 2].Value = "Opis";
                worksheet.Cells[1, 3].Value = "Plan projekta";
                worksheet.Cells[1, 4].Value = "Aktivnost";
                worksheet.Cells[1, 5].Value = "Projekt";

                for (int i = 0; i < etape.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = etape[i].Ime;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment =
                        OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = etape[i].Opis;
                    worksheet.Cells[i + 2, 3].Value = etape[i].PlanProjektaId; // Plan projekta nema niti jedan atribut kojim bih ga mogla prikazati te sam uzela ID 
                    worksheet.Cells[i + 2, 4].Value = etape[i].Aktivnost.Ime;
                    worksheet.Cells[i + 2, 5].Value = etape[i].PlanProjekta.Projekt.Kratica;
                }

                worksheet.Cells[1, 1, etape.Count + 1, 5].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "Etape.xlsx");
        }

        public async Task<IActionResult> PlanoviPDF()
        {
            string naslov = "Planova";
            var plan = await _context.PlanProjekta.AsNoTracking()
                .Include(s => s.Projekt)
                .Include(p => p.Etapa)
                .Include(s => s.VoditeljEmailNavigation)
                .ToListAsync();

            PdfReport report = CreateReport(naslov);

            #region Podnožje i zaglavlje

            report.PagesFooter(footer => { footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy.")); })
                .PagesHeader(header =>
                {
                    header.CacheHeader(cache: true);
                    header.DefaultHeader(defaultHeader =>
                    {
                        defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
                        defaultHeader.Message(naslov);
                    });
                });

            #endregion

            #region Postavljanje izvora podataka i stupaca

            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(plan));

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

                    column.PropertyName<PlanProjekta>(o => o.PlanProjektaId);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(1);
                    column.HeaderCell("ID", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<PlanProjekta>(o => o.PlaniraniPočetak);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Planirani početak", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<PlanProjekta>(o => o.PlaniraniKraj);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(2);
                    column.HeaderCell("Planirani kraj", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<PlanProjekta>(o => o.StvarniPočetak);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(2);
                    column.HeaderCell("Stvarni početak", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<PlanProjekta>(o => o.StvarniKraj);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4);
                    column.Width(2);
                    column.HeaderCell("Stvarni kraj", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<PlanProjekta>(o => o.Projekt.Kratica);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(5);
                    column.Width(2);
                    column.HeaderCell("Projekt", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<PlanProjekta>(o => o.VoditeljEmail);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(6);
                    column.Width(2);
                    column.HeaderCell("Voditelj (email)", horizontalAlignment: HorizontalAlignment.Center);
                });
            });
            #endregion

            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=Planovi.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> EtapePDF()
        {
            string naslov = "Etape";

            var etape = await _context.Etapa.AsNoTracking()
                .Include(p => p.PlanProjekta.Projekt)
                .Include(p => p.Aktivnost)
                .Include(p => p.PlanProjekta)
                .ToListAsync();

            PdfReport report = CreateReport(naslov);

            #region Podnožje i zaglavlje

            report.PagesFooter(footer => { footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy.")); })
                .PagesHeader(header =>
                {
                    header.CacheHeader(cache: true);
                    header.DefaultHeader(defaultHeader =>
                    {
                        defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
                        defaultHeader.Message(naslov);
                    });
                });

            #endregion

            #region Postavljanje izvora podataka i stupaca

            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(etape));

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
                    column.PropertyName<Etapa>(o => o.Ime);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(1);
                    column.HeaderCell("Ime", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Etapa>(o => o.Opis);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Opis", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Etapa>(o => o.PlanProjektaId);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(2);
                    column.HeaderCell("Plan Projekta", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Etapa>(o => o.Aktivnost.Ime);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(2);
                    column.HeaderCell("Aktivnost", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Etapa>(o => o.PlanProjekta.Projekt.Kratica);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4);
                    column.Width(2);
                    column.HeaderCell("Projekt", horizontalAlignment: HorizontalAlignment.Center);
                });
            });
            #endregion

            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=Etape.pdf");
                return File(pdf, "application/pdf");
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
                    Author = "Daria Bevanda",
                    Application = "RPPP",
                    Title = naslov
                });
                doc.Compression(new CompressionSettings
                {
                    EnableCompression = true,
                    EnableFullCompression = true
                });
            })
                .DefaultFonts(fonts =>
                {
                    fonts.Path(Path.Combine(environment.WebRootPath, "fonts", "verdana.ttf"),
                        Path.Combine(environment.WebRootPath, "fonts", "tahoma.ttf"));
                    fonts.Size(9);
                    fonts.Color(System.Drawing.Color.Black);
                })

                .MainTableTemplate(template => { template.BasicTemplate(BasicTemplate.ProfessionalTemplate); })
                .MainTablePreferences(table =>
                {
                    table.ColumnsWidthsType(TableColumnWidthType.Relative);
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
        public async Task<IActionResult> UveziPlan(IFormFile excelFile)
        {
            List<PlanProjekta> planoviN = new List<PlanProjekta>();
            List<PlanProjekta> uspjeh = new List<PlanProjekta>();
            List<PlanProjekta> neuspjeh = new List<PlanProjekta>();
            List<String> broj = new List<String>();

            int counter = 0;

            if (excelFile == null || excelFile.Length == 0)
            {

                ModelState.AddModelError("excelFile", "Odaberite Excel datoteku.");
                return RedirectToAction("UvozIzvoz", "PlanProjekta");
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
                            DateTime planiraniPocetak = DateTime.Parse(worksheet.Cells[row, 1].Value?.ToString());
                            DateTime planiraniZavrsetak = DateTime.Parse(worksheet.Cells[row, 2].Value?.ToString());
                            DateTime stvarniPocetak = DateTime.Parse(worksheet.Cells[row, 3].Value?.ToString());
                            DateTime stvarniZavrsetak = DateTime.Parse(worksheet.Cells[row, 4].Value?.ToString());
                            string voditelj = worksheet.Cells[row, 5].Value?.ToString();
                            string projektKratica = worksheet.Cells[row, 6].Value?.ToString();

                            int idProjekta = _context.Projekt
                                .Where(z => z.Kratica == projektKratica)
                                .Select(z => z.ProjektId)
                                .FirstOrDefault();

                            if (idProjekta != 0)
                            {
                                PlanProjekta novi = new PlanProjekta();
                                novi.PlaniraniPočetak = planiraniPocetak;
                                novi.PlaniraniKraj = planiraniZavrsetak;
                                novi.StvarniPočetak = stvarniPocetak;
                                novi.StvarniKraj = stvarniZavrsetak;
                                novi.ProjektId = idProjekta;
                                novi.VoditeljEmail = voditelj;

                                planoviN.Add(novi);
                                broj.Add("DODAN");
                            }
                            else
                            {
                                broj.Add("NIJE DODAN");
                            }
                        }
                    }
                }

                foreach (var p in planoviN)
                {
                    _context.Add(p);
                    uspjeh.Add(p);
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

                    foreach (var b in broj)
                    {

                        worksheet.Cells[counte2r, brojkol].Value = b;
                        counte2r = counte2r + 1;
                    }

                    byte[] updatedContent = package.GetAsByteArray();
                    Response.Headers.Add("content-disposition", "inline; filename=updatedFile.xlsx");
                    return File(updatedContent, ExcelContentType);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("excelFile", "Došlo je do greške prilikom uvoza podataka iz Excel datoteke.");
                return RedirectToAction("UvozIzvoz", "PlanProjekta");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UveziEtapu(IFormFile excelFile2)
        {
            List<Etapa> etapeN = new List<Etapa>();
            List<Etapa> uspjeh = new List<Etapa>();
            List<Etapa> neuspjeh = new List<Etapa>();
            List<String> broj = new List<String>();

            int counter = 0;

            if (excelFile2 == null || excelFile2.Length == 0)
            {
                TempData["errorMessage"] = "Odaberite Excel datoteku.";
                ModelState.AddModelError("excelFile", "Odaberite Excel datoteku.");
                return RedirectToAction("UvozIzvoz", "PlanProjekta");
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
                            string ime = worksheet.Cells[row, 1].Value?.ToString();
                            string opis = worksheet.Cells[row, 2].Value?.ToString();
                            int planProjektaId = int.Parse(worksheet.Cells[row, 3].Value?.ToString());
                            string aktivnostIme = worksheet.Cells[row, 4].Value?.ToString();

                            int idAktivnost = _context.Aktivnost
                                .Where(z => z.Ime == aktivnostIme)
                                .Select(z => z.AktivnostId)
                                .FirstOrDefault();

                            if (idAktivnost != 0 && ime != null)
                            {
                                Etapa nova = new Etapa();
                                nova.Ime = ime;
                                nova.Opis = opis;
                                nova.PlanProjektaId = planProjektaId;
                                nova.AktivnostId = idAktivnost;
                                etapeN.Add(nova);
                                broj.Add("DODAN");
                            }
                            else
                            {
                                broj.Add("NIJE DODAN");
                            }
                        }
                    }
                }

                foreach (var e in etapeN)
                {
                    _context.Add(e);
                    uspjeh.Add(e);
                }
                await _context.SaveChangesAsync();

                if (uspjeh.Count == broj.Count)
                {
                    var stream = new MemoryStream();
                    await excelFile2.CopyToAsync(stream);
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
                    await excelFile2.CopyToAsync(stream);
                    var package = new ExcelPackage(stream);
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int brojkol = worksheet.Dimension.Columns + 1;

                    foreach (var b in broj)
                    {
                        worksheet.Cells[counte2r, brojkol].Value = b;
                        counte2r = counte2r + 1;
                    }

                    byte[] updatedContent = package.GetAsByteArray();
                    Response.Headers.Add("content-disposition", "inline; filename=updatedFile.xlsx");
                    return File(updatedContent, ExcelContentType);
                }
            }
            catch (Exception ex)
            {
                TempData["errorMessage"] = "Greška.";
                ModelState.AddModelError("excelFile", "Došlo je do greške prilikom uvoza podataka iz Excel datoteke.");
                return RedirectToAction("UvozIzvoz", "PlanProjekta");
            }
        }

        public IActionResult PlanoviTemplate()
        {
            string templateFilePath = "Templates/PlanoviTemplate.xlsx";

            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(System.IO.File.OpenRead(templateFilePath), contentType, "PlanoviTemplate.xlsx");
        }

        public IActionResult EtapeTemplate()
        {
            string templateFilePath = "Templates/EtapeTemplate.xlsx";

            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(System.IO.File.OpenRead(templateFilePath), contentType, "EtapeTemplate.xlsx");
        }
    }
}