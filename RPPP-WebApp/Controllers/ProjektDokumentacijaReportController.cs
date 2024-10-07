using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PdfRpt.Core.Contracts;
using PdfRpt.FluentInterface;
using RPPP_WebApp.Data;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using System.Text;

namespace RPPP_WebApp.Controllers;

/// <summary>
/// Controller koji služi za obradu zahtjeva vezanih za izradu reporta o dokumentaciji i projektima
/// </summary>
public class ProjektDokumentacijaReportController : Controller
{

    private readonly RPPP06Context _context;
    private readonly IWebHostEnvironment environment;
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    /// <summary>
    /// Konstruktor razreda ProjektDokumentacijaReportController.
    /// </summary>
    /// <param name="context">Kontekst baze podataka.</param>
    /// <param name="environment">Varijabla okruženja tipa IWebHostEnvironment predstavlja instancu sučelja IWebHostEnvironment</param>
    public ProjektDokumentacijaReportController(RPPP06Context context, IWebHostEnvironment environment)
    {
        _context = context;
        this.environment = environment;
    }

    /// <summary>
    /// Endpoint koji služi za pristup ImportExport view-u
    /// </summary>
    /// <returns>View ImportExport</returns>
    [HttpGet]
    public async Task<IActionResult> ImportExport()
    {
        return View();
    }

    /// <summary>
    /// Endpoint koji služi za kreaciju datoteke u excelu koja prikazuje sve master detail podatke
    /// </summary>
    /// <returns>Excel file sa svim master detail podatcima projekta</returns>
    [HttpGet]
    public async Task<IActionResult> MasterDetailExport()
    {
        var projekti = await _context.Projekt.AsNoTracking()
        .Include(p => p.VrstaProjekta)
        .Include(p => p.ProjektnaKartica)
        .Include(p => p.Dokumentacija)
        .Include(p => p.PlanProjekta)
        .Include(p => p.SuradnikEmail)
        .ToListAsync();

        byte[] content;

        using (ExcelPackage excel = new ExcelPackage())
        {
            excel.Workbook.Properties.Title = "Popis projekata i dokumentacija";
            excel.Workbook.Properties.Author = "Mario Mrvčić";
            var worksheet = excel.Workbook.Worksheets.Add("Projekti");

            worksheet.Cells[1, 1].Value = "Projekt Id";
            worksheet.Cells[1, 2].Value = "Ime";
            worksheet.Cells[1, 3].Value = "Kratica";
            worksheet.Cells[1, 4].Value = "Opis";
            worksheet.Cells[1, 5].Value = "Planirani Početaka";
            worksheet.Cells[1, 6].Value = "Planirani Kraj";
            worksheet.Cells[1, 7].Value = "Stvarni Početak";
            worksheet.Cells[1, 8].Value = "Stvarni Kraj";
            worksheet.Cells[1, 9].Value = "Vrsta Projekta";
            worksheet.Cells[1, 10].Value = "Dokumentacija";
            worksheet.Cells[1, 11].Value = "Plan Projekta";
            worksheet.Cells[1, 12].Value = "Projektna Kartica";
            worksheet.Cells[1, 13].Value = "Suradnik Email";

            for (int i = 0; i < projekti.Count; i++)
            {
                var project = projekti[i];

                worksheet.Cells[i + 2, 1].Value = project.ProjektId;
                worksheet.Cells[i + 2, 2].Value = project.Ime;
                worksheet.Cells[i + 2, 3].Value = project.Kratica;
                worksheet.Cells[i + 2, 4].Value = project.Opis;
                worksheet.Cells[i + 2, 5].Value = project.PlaniraniPočetak.ToString("yyyy-MM-dd");
                worksheet.Cells[i + 2, 6].Value = project.PlaniraniKraj.ToString("yyyy-MM-dd");
                worksheet.Cells[i + 2, 7].Value = project.StvarniPočetak?.ToString("yyyy-MM-dd");
                worksheet.Cells[i + 2, 8].Value = project.StvarniKraj?.ToString("yyyy-MM-dd");
                worksheet.Cells[i + 2, 9].Value = project.VrstaProjekta?.Ime;
                worksheet.Cells[i + 2, 10].Value = string.Join(", ", project.Dokumentacija.Select(d => d.NazivDokumentacije));
                worksheet.Cells[i + 2, 11].Value = project.PlanProjekta?.ProjektId;
                worksheet.Cells[i + 2, 12].Value = string.Join(", ", project.ProjektnaKartica.Select(pk => pk.Iban));
                worksheet.Cells[i + 2, 13].Value = string.Join(", ", project.SuradnikEmail.Select(s => s.Email));

            }

            worksheet.Cells[1, 1, projekti.Count + 1, 13].AutoFitColumns();

            content = excel.GetAsByteArray();
        }

        return File(content, ExcelContentType, "ProjektiMD.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ProjektiExport()
    {
        var projekti = await _context.Projekt.AsNoTracking()
        .Include(p => p.VrstaProjekta)
        .ToListAsync();

        byte[] content;

        using (ExcelPackage excel = new ExcelPackage())
        {
            excel.Workbook.Properties.Title = "Popis projekata";
            excel.Workbook.Properties.Author = "Mario Mrvčić";
            var worksheet = excel.Workbook.Worksheets.Add("Projekti");

            worksheet.Cells[1, 1].Value = "Projekt Id";
            worksheet.Cells[1, 2].Value = "Ime";
            worksheet.Cells[1, 3].Value = "Kratica";
            worksheet.Cells[1, 4].Value = "Opis";
            worksheet.Cells[1, 5].Value = "Planirani Početaka";
            worksheet.Cells[1, 6].Value = "Planirani Kraj";
            worksheet.Cells[1, 7].Value = "Stvarni Početak";
            worksheet.Cells[1, 8].Value = "Stvarni Kraj";
            worksheet.Cells[1, 9].Value = "Vrsta Projekta";

            for (int i = 0; i < projekti.Count; i++)
            {
                var project = projekti[i];

                worksheet.Cells[i + 2, 1].Value = project.ProjektId;
                worksheet.Cells[i + 2, 2].Value = project.Ime;
                worksheet.Cells[i + 2, 3].Value = project.Kratica;
                worksheet.Cells[i + 2, 4].Value = project.Opis;
                worksheet.Cells[i + 2, 5].Value = project.PlaniraniPočetak.ToString("yyyy-MM-dd");
                worksheet.Cells[i + 2, 6].Value = project.PlaniraniKraj.ToString("yyyy-MM-dd");
                worksheet.Cells[i + 2, 7].Value = project.StvarniPočetak?.ToString("yyyy-MM-dd");
                worksheet.Cells[i + 2, 8].Value = project.StvarniKraj?.ToString("yyyy-MM-dd");
                worksheet.Cells[i + 2, 9].Value = project.VrstaProjekta?.Ime;
            }

            worksheet.Cells[1, 1, projekti.Count + 1, 9].AutoFitColumns();

            content = excel.GetAsByteArray();
        }

        return File(content, ExcelContentType, "ProjektiSTP.xlsx");
    }

    /// <summary>
    /// Endpoint koji služi za kreaciju excel file-a koji sadrži svu dokumentaciju
    /// </summary>
    /// <returns>Excel file sa svim podatcima o dokumentaciji</returns>
    public async Task<IActionResult> DokumentacijaExport()
    {
        var dokumenti = await _context.Dokumentacija.AsNoTracking()
            .Include(d => d.VrstaDokumentacije)
            .Include(d => d.Projekt)
            .ToListAsync();

        byte[] content;

        using (ExcelPackage excel = new ExcelPackage())
        {
            excel.Workbook.Properties.Title = "Popis dokumentacije";
            excel.Workbook.Properties.Author = "Mario Mrvčić";
            var worksheet = excel.Workbook.Worksheets.Add("Dokumentacija");

            worksheet.Cells[1, 1].Value = "Dokumentacija Id";
            worksheet.Cells[1, 2].Value = "Naziv Dokumentacije";
            worksheet.Cells[1, 3].Value = "Vrijeme Kreacije";
            worksheet.Cells[1, 4].Value = "Format";
            worksheet.Cells[1, 5].Value = "URL";
            worksheet.Cells[1, 6].Value = "Status Dovršenosti";
            worksheet.Cells[1, 7].Value = "Projekt";
            worksheet.Cells[1, 8].Value = "VrstaDokumentacije";

            for (int i = 0; i < dokumenti.Count; i++)
            {
                var dokumentacija = dokumenti[i];

                worksheet.Cells[i + 2, 1].Value = dokumentacija.DokumentacijaId;
                worksheet.Cells[i + 2, 2].Value = dokumentacija.NazivDokumentacije;
                worksheet.Cells[i + 2, 3].Value = dokumentacija.VrijemeKreacije.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cells[i + 2, 4].Value = dokumentacija.Format;
                worksheet.Cells[i + 2, 5].Value = dokumentacija.URL;
                worksheet.Cells[i + 2, 6].Value = dokumentacija.StatusDovršenosti;
                worksheet.Cells[i + 2, 7].Value = dokumentacija.Projekt?.Ime;
                worksheet.Cells[i + 2, 8].Value = dokumentacija.VrstaDokumentacije?.Ime;
            }

            worksheet.Cells[1, 1, dokumenti.Count + 1, 8].AutoFitColumns();

            content = excel.GetAsByteArray();
        }

        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Dokumentacija.xlsx");
    }

    /// <summary>
    /// Endpoint koji služi za kreaciju pdf file-a koji sadrži sve projekte
    /// </summary>
    /// <returns>Pdf file sa svim podatcima o projektima</returns>
    [HttpGet]
    public async Task<IActionResult> ProjektiPdfExport()
    {
        string naslov = "Popis projekata";
        var projekti = await _context.Projekt.AsNoTracking()
            .Include(p => p.VrstaProjekta)
            .Include(p => p.ProjektnaKartica)
            .Include(p => p.Dokumentacija)
            .Include(p => p.PlanProjekta)
            .Include(p => p.SuradnikEmail)
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

        report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(projekti));

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
                column.PropertyName<Projekt>(p => p.ProjektId);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(1);
                column.Width(2);
                column.HeaderCell("Ime projekta", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Projekt>(p => p.Ime);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(2);
                column.HeaderCell("Ime projekta", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Projekt>(p => p.Kratica);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(3);
                column.Width(2);
                column.HeaderCell("Kratica", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Projekt>(p => p.Opis);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(4);
                column.Width(2);
                column.HeaderCell("Opis", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Projekt>(p => p.PlaniraniPočetak);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(5);
                column.Width(2);
                column.HeaderCell("Planirani Početak", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Projekt>(p => p.PlaniraniKraj);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(6);
                column.Width(2);
                column.HeaderCell("Planirani Kraj", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Projekt>(p => p.StvarniPočetak);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(7);
                column.Width(2);
                column.HeaderCell("Stvarni Početak", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Projekt>(p => p.StvarniKraj);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(8);
                column.Width(2);
                column.HeaderCell("Stvarni Kraj", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Projekt>(p => p.VrstaProjekta.Ime);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(9);
                column.Width(2);
                column.HeaderCell("Vrsta Projekta", horizontalAlignment: HorizontalAlignment.Center);
            });

        });

        #endregion

        byte[] pdf = report.GenerateAsByteArray();

        if (pdf != null)
        {
            Response.Headers.Add("content-disposition", "inline; filename=Projekti.pdf");
            return File(pdf, "application/pdf");
        }
        else
        {
            return NotFound();
        }
    }


    /// <summary>
    /// Endpoint koji služi za kreaciju pdf file-a koji sadrži svu dokumentaciju
    /// </summary>
    /// <returns>pdf file sa svim podatcima o dokumentaciji</returns>
    [HttpGet]
    public async Task<IActionResult> DokumentacijaPdfExport()
    {
        string naslov = "Popis dokumentacije";
        var dokumentacija = await _context.Dokumentacija.AsNoTracking()
            .Include(d => d.VrstaDokumentacije)
            .Include(d => d.Projekt)
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

        report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(dokumentacija));

        report.MainTableColumns(columns =>
        {
            columns.AddColumn(column =>
            {
                column.PropertyName<Dokumentacija>(d => d.DokumentacijaId);
                column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                column.IsVisible(true);
                column.Order(0);
                column.Width(1);
                column.HeaderCell("Dokumentacija Id", horizontalAlignment: HorizontalAlignment.Right);
            });

            columns.AddColumn(column =>
            {
                column.IsRowNumber(true);
                column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                column.IsVisible(true);
                column.Order(1);
                column.Width(1);
                column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Dokumentacija>(d => d.NazivDokumentacije);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(2);
                column.HeaderCell("Naziv Dokumentacije", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Dokumentacija>(d => d.VrijemeKreacije);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(3);
                column.Width(2);
                column.HeaderCell("Vrijeme Kreacije", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Dokumentacija>(d => d.Format);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(4);
                column.Width(2);
                column.HeaderCell("Format", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Dokumentacija>(d => d.URL);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(5);
                column.Width(2);
                column.HeaderCell("URL", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Dokumentacija>(d => d.StatusDovršenosti);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(6);
                column.Width(2);
                column.HeaderCell("Status Dovršenosti", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Dokumentacija>(d => d.Projekt.Ime);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(7);
                column.Width(2);
                column.HeaderCell("Projekt", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Dokumentacija>(d => d.VrstaDokumentacije.Ime);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(8);
                column.Width(2);
                column.HeaderCell("Vrsta Dokumentacije", horizontalAlignment: HorizontalAlignment.Center);
            });


        });

        #endregion

        byte[] pdf = report.GenerateAsByteArray();

        if (pdf != null)
        {
            Response.Headers.Add("content-disposition", "inline; filename=Dokumentacija.pdf");
            return File(pdf, "application/pdf");
        }
        else
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Endpoint koji služi za kreaciju pdf file-a
    /// </summary>
    /// <param name="naslov">Naslov za pdf file</param>
    /// <returns></returns>
    private PdfReport CreateReport(string naslov)
    {
        var pdf = new PdfReport();

        pdf.DocumentPreferences(doc =>
        {
            doc.Orientation(PageOrientation.Portrait);
            doc.PageSize(PdfPageSize.A4);
            doc.DocumentMetadata(new DocumentMetadata
            {
                Author = "Mario Mrvčić",
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


    /// <summary>
    /// Endpoint koji služi za importiranje excel file-a sa projektima koje želimo dodati
    /// </summary>
    /// <returns>Excel file sa povratnom informacijom o dodanim projektima ili redirect na ImportExport view</returns>
    [HttpPost]
    public async Task<IActionResult> ImportProjekt(IFormFile excelFile)
    {
        List<Projekt> projektiNovi = new List<Projekt>();
        List<Projekt> uspjeh = new List<Projekt>();
        List<Projekt> neuspjeh = new List<Projekt>();
        List<string> broj = new List<string>();

        if (excelFile == null || excelFile.Length == 0)
        {
            ModelState.AddModelError("excelFile", "Odaberite Excel datoteku.");
            return RedirectToAction("ImportExport", "ProjektDokumentacijaReport");
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
                        string Ime = worksheet.Cells[row, 1].Value.ToString();
                        string Kratica = worksheet.Cells[row, 2].Value.ToString();
                        string Opis = worksheet.Cells[row, 3].Value.ToString();
                        DateTime PlaniraniPočetak = Convert.ToDateTime(worksheet.Cells[row, 4].Value);
                        DateTime PlaniraniKraj = Convert.ToDateTime(worksheet.Cells[row, 5].Value);
                        DateTime? StvarniPočetak = string.IsNullOrWhiteSpace(worksheet.Cells[row, 6]?.Value?.ToString()) ? null : (DateTime?)Convert.ToDateTime(worksheet.Cells[row, 6]?.Value);
                        DateTime? StvarniKraj = string.IsNullOrWhiteSpace(worksheet.Cells[row, 7]?.Value?.ToString()) ? null : (DateTime?)Convert.ToDateTime(worksheet.Cells[row, 7].Value);
                        string VrstaProjektaName = worksheet.Cells[row, 8].Value.ToString(); // Assuming user provides project type name

                        int? VrstaProjektaId = _context.VrstaProjekta.FirstOrDefault(vp => vp.Ime == VrstaProjektaName)?.VrstaProjektaId;

                        if (!string.IsNullOrWhiteSpace(Ime) && !string.IsNullOrWhiteSpace(Kratica) && !string.IsNullOrWhiteSpace(Opis) && VrstaProjektaId.HasValue)
                        {
                            Projekt projekt = new Projekt
                            {
                                Ime = Ime,
                                Kratica = Kratica,
                                Opis = Opis,
                                PlaniraniPočetak = PlaniraniPočetak,
                                PlaniraniKraj = PlaniraniKraj,
                                StvarniPočetak = StvarniPočetak,
                                StvarniKraj = StvarniKraj,
                                VrstaProjektaId = VrstaProjektaId.Value
                            };

                            var postoji = _context.Projekt
                                .Where(p => p.Ime == projekt.Ime && p.Kratica == projekt.Kratica)
                                .FirstOrDefault();

                            if (postoji != null)
                            {
                                broj.Add("Nije dodan");
                            }
                            else
                            {
                                projektiNovi.Add(projekt);
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

            foreach (var projekt in projektiNovi)
            {
                _context.Add(projekt);
                uspjeh.Add(projekt);
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

                foreach (var status in broj)
                {
                    worksheet.Cells[counte2r, brojkol].Value = status;
                    counte2r = counte2r + 1;
                }

                byte[] updatedContent = package.GetAsByteArray();
                Response.Headers.Add("content-disposition", "inline; filename=updatedFile.xlsx");
                return File(updatedContent, ExcelContentType);
            }
        }
        catch (Exception ex)
        {
            TempData["errorMessage"] = "Dogodila se greška!";
            return RedirectToAction("ImportExport", "ProjektDokumentacijaReport");
        }
    }

    /// <summary>
    /// Endpoint koji služi za importiranje excel file-a sa dokumentacijom koju želimo dodati
    /// </summary>
    /// <returns>Excel file sa povratnom informacijom o dodanim podatcima ili redirect na ImportExport view</returns>
    [HttpPost]
    public async Task<IActionResult> ImportDokumentacija(IFormFile excelFile)
    {
        List<Dokumentacija> dokumentacijeNovi = new List<Dokumentacija>();
        List<Dokumentacija> uspjeh = new List<Dokumentacija>();
        List<Dokumentacija> neuspjeh = new List<Dokumentacija>();
        List<string> broj = new List<string>();

        if (excelFile == null || excelFile.Length == 0)
        {
            ModelState.AddModelError("excelFile", "Odaberite Excel datoteku.");
            return RedirectToAction("ImportExport", "ProjektDokumentacijaReport");
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
                        string NazivDokumentacije = worksheet.Cells[row, 1].Value.ToString();
                        string Format = worksheet.Cells[row, 2].Value.ToString();
                        string URL = worksheet.Cells[row, 3].Value.ToString();
                        string StatusDovršenosti = worksheet.Cells[row, 4].Value.ToString();
                        string ProjektName = worksheet.Cells[row, 5].Value.ToString();
                        string VrstaDokumentacijeName = worksheet.Cells[row, 6].Value.ToString();

                        int? ProjektId = _context.Projekt.FirstOrDefault(p => p.Ime == ProjektName)?.ProjektId;
                        int? VrstaDokumentacijeId = _context.VrstaDokumentacije.FirstOrDefault(v => v.Ime == VrstaDokumentacijeName)?.VrstaDokumentacijeId;

                        if (!string.IsNullOrWhiteSpace(NazivDokumentacije) && !string.IsNullOrWhiteSpace(URL) && ProjektId.HasValue && VrstaDokumentacijeId.HasValue)
                        {
                            Dokumentacija dokumentacija = new Dokumentacija
                            {
                                NazivDokumentacije = NazivDokumentacije,
                                VrijemeKreacije = DateTime.Now,
                                Format = Format,
                                URL = URL,
                                StatusDovršenosti = StatusDovršenosti,
                                ProjektId = ProjektId.Value,
                                VrstaDokumentacijeId = VrstaDokumentacijeId.Value
                            };

                            var postoji = _context.Dokumentacija
                                .Where(d => d.NazivDokumentacije == dokumentacija.NazivDokumentacije)
                                .FirstOrDefault();

                            if (postoji != null)
                            {
                                broj.Add("Nije dodana");
                            }
                            else
                            {
                                dokumentacijeNovi.Add(dokumentacija);
                                broj.Add("Dodana");
                            }
                        }
                        else
                        {
                            broj.Add("Nije dodana");
                        }
                    }
                }
            }

            foreach (var dokumentacija in dokumentacijeNovi)
            {
                _context.Add(dokumentacija);
                uspjeh.Add(dokumentacija);
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
                    worksheet.Cells[i, worksheet.Dimension.Columns + 1].Value = "DODANA";
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

                foreach (var status in broj)
                {
                    worksheet.Cells[counte2r, brojkol].Value = status;
                    counte2r = counte2r + 1;
                }

                byte[] updatedContent = package.GetAsByteArray();
                Response.Headers.Add("content-disposition", "inline; filename=updatedFile.xlsx");
                return File(updatedContent, ExcelContentType);
            }
        }
        catch (Exception ex)
        {
            TempData["errorMessage"] = "Dogodila se greška!";
            return RedirectToAction("ImportExport", "ProjektDokumentacijaReport");
        }
    }

    /// <summary>
    /// Endpoint koji dohvaća template za import podataka projekta
    /// </summary>
    /// <returns>Excel template za import podataka projekta</returns>
    [HttpGet]
    public IActionResult ProjektiTemplate()
    {
        string templateFilePath = "Templates/ProjektiTemplate.xlsx";

        string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        return File(System.IO.File.OpenRead(templateFilePath), contentType, "ProjektiTemplate.xlsx");
    }

    /// <summary>
    /// Endpoint koji dohvaća template za import podataka dokumentacije
    /// </summary>
    /// <returns>Excel template za import podataka dokumentacije</returns>
    [HttpGet]
    public IActionResult DokumentacijaTemplate()
    {
        string templateFilePath = "Templates/DokumentacijaTemplate.xlsx";

        string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        return File(System.IO.File.OpenRead(templateFilePath), contentType, "DokumentacijaTemplate.xlsx");
    }

}
