using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PdfRpt.FluentInterface;
using PdfRpt.Core.Contracts;
using RPPP_WebApp.Data;
using RPPP_WebApp.Models;
using System.Text;
using SkiaSharp;

namespace RPPP_WebApp.Controllers
{
    public class ZahtjevReport : Controller
    {
        private readonly RPPP06Context _context;
        private readonly IWebHostEnvironment environment;
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public ZahtjevReport(RPPP06Context context, IWebHostEnvironment environment)
        {
            _context = context;
            this.environment = environment;
        }

        public async Task<IActionResult> ZahtjevMD2()
        {
            var Zahtjev = await _context.Zahtjev.AsNoTracking()
                .Include(s => s.Zadatak)
                .Include(p => p.TipZahtjeva)
                .Include(s => s.Prioritet)
                .Include(s => s.PlanProjekta)
                    .ThenInclude(s => s.Projekt)
                .ToListAsync();

            byte[] content;

            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Izvještaj o zahtjevima te njihovim zadatcima";
                excel.Workbook.Properties.Author = "Fran Talan";

                for (int i = 0; i < Zahtjev.Count; i++)
                {
                    var title = Zahtjev[i].ZahtjevId.ToString();
                    var worksheet = excel.Workbook.Worksheets.Add(title);

                    worksheet.Cells[1, 1].Value = "Zahtjev Id";
                    worksheet.Cells[1, 2].Value = "Ime";
                    worksheet.Cells[1, 3].Value = "Opis";
                    worksheet.Cells[1, 4].Value = "Projekt";
                    worksheet.Cells[1, 5].Value = "Prioritet";
                    worksheet.Cells[1, 6].Value = "Tip Zahtjeva";
                    worksheet.Cells[1, 7].Value = "Zadatci";

                    worksheet.Cells[2, 1].Value = Zahtjev[i].ZahtjevId;
                    worksheet.Cells[2, 1].Style.HorizontalAlignment =
                        OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[2, 2].Value = Zahtjev[i].Ime;
                    worksheet.Cells[2, 3].Value = Zahtjev[i].Opis;
                    worksheet.Cells[2, 4].Value = Zahtjev[i].PlanProjekta.Projekt.Ime;
                    worksheet.Cells[2, 5].Value = Zahtjev[i].Prioritet.Ime;
                    worksheet.Cells[2, 6].Value = Zahtjev[i].TipZahtjeva.Ime;

                    // Details
                    List<Zadatak> zadatci = Zahtjev[i].Zadatak.ToList();

                    StringBuilder builderEx = new StringBuilder();

                    if (zadatci.Count == 0)
                    {
                        worksheet.Cells[2, 7].Value = "-";
                    }
                    else
                    {
                        foreach (var e in zadatci)
                        {
                            builderEx.Append(e.OpisZadatka + ", ");
                        }

                        if (builderEx.ToString().Contains(", "))
                        {
                            builderEx.Remove(builderEx.Length - 2, 2);
                        }

                        worksheet.Cells[2, 7].Value = builderEx;
                    }

                    worksheet.Cells[1, 1, 1, 7].AutoFitColumns();
                }

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "ZahtjevMD.xlsx");
        }


        public async Task<IActionResult> Zahtjevi()
        {
            var zahtjev = await _context.Zahtjev.AsNoTracking()
                .Include(p => p.TipZahtjeva)
                .Include(s => s.Prioritet)
                .Include(s => s.PlanProjekta)
                    .ThenInclude(s => s.Projekt)
                .ToListAsync();

            byte[] content;

            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Izvještaj o zahtjevima te njihovim zadatcima";
                excel.Workbook.Properties.Author = "Fran Talan";
                var worksheet = excel.Workbook.Worksheets.Add("Zahtjevi");

                worksheet.Cells[1, 1].Value = "Zahtjev Id";
                worksheet.Cells[1, 2].Value = "Ime";
                worksheet.Cells[1, 3].Value = "Opis";
                worksheet.Cells[1, 4].Value = "Projekt";
                worksheet.Cells[1, 5].Value = "Prioritet";
                worksheet.Cells[1, 6].Value = "Tip Zahtjeva";


                for (int i = 0; i < zahtjev.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = zahtjev[i].ZahtjevId;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment =
                        OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = zahtjev[i].Ime;
                    worksheet.Cells[i + 2, 3].Value = zahtjev[i].Opis;
                    worksheet.Cells[i + 2, 4].Value = zahtjev[i].PlanProjekta.Projekt.Ime;
                    worksheet.Cells[i + 2, 5].Value = zahtjev[i].Prioritet.Ime;
                    worksheet.Cells[i + 2, 6].Value = zahtjev[i].TipZahtjeva.Ime;
                }

                worksheet.Cells[1, 1, zahtjev.Count + 1, 6].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "Zahtjevi.xlsx");
        }

        public async Task<IActionResult> Zadatci()
        {
            var zadatci = await _context.Zadatak.AsNoTracking()
                .Include(p => p.Status)
                .Include(p => p.Posao)
                .Include(p => p.Zahtjev)
                .ToListAsync();

            byte[] content;

            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Zadatci";
                excel.Workbook.Properties.Author = "Fran Talan";
                var worksheet = excel.Workbook.Worksheets.Add("Zadatci");

                worksheet.Cells[1, 1].Value = "Zadatak Id";
                worksheet.Cells[1, 2].Value = "OpisZadatka";
                worksheet.Cells[1, 3].Value = "PlaniraniPočetak";
                worksheet.Cells[1, 4].Value = "PlaniraniKraj";
                worksheet.Cells[1, 5].Value = "StvarniPočetak";
                worksheet.Cells[1, 6].Value = "StvarniKraj";
                worksheet.Cells[1, 7].Value = "Status";
                worksheet.Cells[1, 8].Value = "Zastavica Aktivnosti";
                worksheet.Cells[1, 9].Value = "Nositelj Email";

                for (int i = 0; i < zadatci.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = zadatci[i].ZadatakId;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment =
                        OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = zadatci[i].OpisZadatka;
                    worksheet.Cells[i + 2, 3].Value = zadatci[i].PlaniraniPočetak.ToString("dd-MM-yyyy HH:mm");
                    worksheet.Cells[i + 2, 4].Value = zadatci[i].PlaniraniKraj.ToString("dd-MM-yyyy HH:mm");
                    worksheet.Cells[i + 2, 5].Value = zadatci[i].StvarniPočetak?.ToString("dd-MM-yyyy HH:mm");
                    worksheet.Cells[i + 2, 6].Value = zadatci[i].StvarniKraj?.ToString("dd-MM-yyyy HH:mm");
                    worksheet.Cells[i + 2, 7].Value = zadatci[i].Status.Ime;
                    worksheet.Cells[i + 2, 8].Value = zadatci[i].Status.ZastavicaAktivnosti;
                    worksheet.Cells[i + 2, 9].Value = zadatci[i].NositeljEmail;
                }

                worksheet.Cells[1, 1, zadatci.Count + 1, 9].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "Zadatci.xlsx");
        }

        public async Task<IActionResult> ZahtjeviPDF()
        {
            string naslov = "Zahtjev";
            var zahtjev = await _context.Zahtjev.AsNoTracking()
                .Include(p => p.TipZahtjeva)
                .Include(s => s.Prioritet)
                .Include(s => s.PlanProjekta)
                    .ThenInclude(s => s.Projekt)
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

            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(zahtjev));

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

                    column.PropertyName<Zahtjev>(o => o.ZahtjevId);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(1);
                    column.HeaderCell("Zahtjev Id", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Zahtjev>(o => o.Ime);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Ime", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Zahtjev>(o => o.Opis);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(2);
                    column.HeaderCell("Opis", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Zahtjev>(o => o.PlanProjekta.Projekt.Ime);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(2);
                    column.HeaderCell("Projekt", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Zahtjev>(o => o.Prioritet.Ime);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4);
                    column.Width(2);
                    column.HeaderCell("Prioritet", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Zahtjev>(o => o.TipZahtjeva.Ime);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(5);
                    column.Width(2);
                    column.HeaderCell("Tip Zahtjeva", horizontalAlignment: HorizontalAlignment.Center);
                });
            });
            #endregion

            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=Zahtjevi.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> ZadatciPDF()
        {
            string naslov = "Zadatci";

            var zadatci = await _context.Zadatak.AsNoTracking()
                .Include(p => p.Status)
                .Include(p => p.Posao)
                .Include(p => p.Zahtjev)
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

            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(zadatci));

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
                    column.PropertyName<Zadatak>(o => o.ZadatakId);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(1);
                    column.HeaderCell("Zadatak Id", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Zadatak>(o => o.OpisZadatka);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Opis Zadatka", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Zadatak>(o => o.PlaniraniPočetak);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(2);
                    column.HeaderCell("Planirani Početak", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Zadatak>(o => o.PlaniraniKraj);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(2);
                    column.HeaderCell("Planirani Kraj", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Zadatak>(o => o.StvarniPočetak);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4);
                    column.Width(2);
                    column.HeaderCell("Stvarni Početak", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Zadatak>(o => o.StvarniKraj);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(5);
                    column.Width(2);
                    column.HeaderCell("Stvarni Kraj", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Zadatak>(o => o.Status.Ime);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(6);
                    column.Width(2);
                    column.HeaderCell("Status", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Zadatak>(o => o.Status.ZastavicaAktivnosti);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(7);
                    column.Width(2);
                    column.HeaderCell("Zastavica Aktivnosti", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Zadatak>(o => o.NositeljEmail);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(8);
                    column.Width(2);
                    column.HeaderCell("Nositelj Email", horizontalAlignment: HorizontalAlignment.Center);
                });
            });
            #endregion

            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=Zadatci.pdf");
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
                    Author = "Fran Talan",
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
        public async Task<IActionResult> UveziZahtjev(IFormFile excelFile)
        {
            List<Zahtjev> zahtjeviN = new List<Zahtjev>();
            List<Zahtjev> uspjeh = new List<Zahtjev>();
            List<Zahtjev> neuspjeh = new List<Zahtjev>();
            List<String> broj = new List<String>();

            int counter = 0;

            if (excelFile == null || excelFile.Length == 0)
            {

                ModelState.AddModelError("excelFile", "Odaberite Excel datoteku.");
                return RedirectToAction("UvozIzvoz", "Zahtjev");
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
                            string ime = worksheet.Cells[row, 1].Value?.ToString();
                            string opis = worksheet.Cells[row, 2].Value?.ToString();
                            string projektIme = worksheet.Cells[row, 3].Value?.ToString();
                            string prioritetIme = worksheet.Cells[row, 4].Value?.ToString();
                            string tipZahtjevaIme = worksheet.Cells[row, 5].Value?.ToString();

                            int idPlanaProjekta = _context.Projekt
                                .Where(z => z.Ime == projektIme)
                                .Select(z => z.PlanProjekta.PlanProjektaId)
                                .FirstOrDefault();

                            int idPrioriteta = _context.Prioritet
                                .Where(z => z.Ime == prioritetIme)
                                .Select(z => z.PrioritetId)
                                .FirstOrDefault();

                            int idTipZahtjeva = _context.TipZahtjeva
                                .Where(z => z.Ime == tipZahtjevaIme)
                                .Select(z => z.TipZahtjevaId)
                                .FirstOrDefault();

                            if (idPlanaProjekta != 0 && idPrioriteta != 0 && idTipZahtjeva != 0)
                            {
                                Zahtjev noviZahtjev = new Zahtjev();
                                noviZahtjev.Ime = ime;
                                noviZahtjev.Opis = opis;
                                noviZahtjev.PlanProjektaId = idPlanaProjekta;
                                noviZahtjev.PrioritetId = idPrioriteta;
                                noviZahtjev.TipZahtjevaId = idTipZahtjeva;

                                zahtjeviN.Add(noviZahtjev);
                                broj.Add("DODAN");
                            }
                            else
                            {
                                broj.Add("NIJE DODAN");
                            }
                        }
                    }
                }

                foreach (var p in zahtjeviN)
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
                return RedirectToAction("UvozIzvoz", "Zahtjev");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UveziZadatak(IFormFile excelFile2)
        {
            List<Zadatak> zadatciN = new List<Zadatak>();
            List<Zadatak> uspjeh = new List<Zadatak>();
            List<Zadatak> neuspjeh = new List<Zadatak>();
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
                            string opis = worksheet.Cells[row, 1].Value?.ToString();
                            DateTime planiraniPocetak = DateTime.Parse(worksheet.Cells[row, 2].Value?.ToString());
                            DateTime planiraniZavrsetak = DateTime.Parse(worksheet.Cells[row, 3].Value?.ToString());
                            DateTime stvarniPocetak = DateTime.Parse(worksheet.Cells[row, 4].Value?.ToString());
                            DateTime stvarniZavrsetak = DateTime.Parse(worksheet.Cells[row, 5].Value?.ToString());
                            string statusIme = worksheet.Cells[row, 6].Value?.ToString();
                            string nositeljEmail = worksheet.Cells[row, 7].Value?.ToString();
                            string zahtjevIme = worksheet.Cells[row, 8].Value?.ToString();

                            int idStatus = _context.Status
                                .Where(z => z.Ime == statusIme)
                                .Select(z => z.StatusId)
                                .FirstOrDefault();

                            int idZahtjeva = _context.Zahtjev
                                .Where(z => z.Ime == zahtjevIme)
                                .Select(z => z.ZahtjevId)
                                .FirstOrDefault();

                            if (idStatus != 0)
                            {
                                Zadatak zadatak = new Zadatak();
                                zadatak.OpisZadatka = opis;
                                zadatak.PlaniraniPočetak = planiraniPocetak;
                                zadatak.PlaniraniKraj = planiraniZavrsetak;
                                zadatak.StvarniPočetak = stvarniPocetak;
                                zadatak.StvarniKraj = stvarniZavrsetak;
                                zadatak.StatusId = idStatus;
                                zadatak.NositeljEmail = nositeljEmail;
                                zadatak.ZahtjevId = idZahtjeva;
                                zadatciN.Add(zadatak);
                                broj.Add("DODAN");
                            }
                            else
                            {
                                broj.Add("NIJE DODAN");
                            }
                        }
                    }
                }

                foreach (var e in zadatciN)
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
                return RedirectToAction("UvozIzvoz", "Zahtjev");
            }
        }

        public IActionResult ZahtjevTemplate()
        {
            string templateFilePath = "Templates/ZahtjevTemplate.xlsx";

            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(System.IO.File.OpenRead(templateFilePath), contentType, "ZahtjevTemplate.xlsx");
        }

        public IActionResult ZadatakTemplate()
        {
            string templateFilePath = "Templates/ZadatakTemplate.xlsx";

            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(System.IO.File.OpenRead(templateFilePath), contentType, "ZadatakTemplate.xlsx");
        }
    }
}