using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PdfRpt.Core.Contracts;
using PdfRpt.FluentInterface;
using RPPP_WebApp.Data;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using System.Text;

namespace RPPP_WebApp.Controllers
{
    public class SuradnikReport : Controller
    {
        private readonly RPPP06Context _context;
        private readonly IWebHostEnvironment environment;
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public SuradnikReport(RPPP06Context context, IWebHostEnvironment environment)
        {
            _context = context;
            this.environment = environment;
        }

        public async Task<IActionResult> SuradnikMD2()
        {
            var suradnik = await _context.Suradnik.AsNoTracking()
                .Include(s => s.Uloga)
                .Include(s => s.Posao)
                .Include(s => s.Zadatak)
                .Include(s => s.NadređeniEmailNavigation)
                .Include(s => s.Projekt)
                .ToListAsync();

            byte[] content;

            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis suradnika i poslova";
                excel.Workbook.Properties.Author = "Marko Ćurković";
                var worksheet = excel.Workbook.Worksheets.Add("Suradnici");

                worksheet.Cells[1, 1].Value = "Ime";
                worksheet.Cells[1, 2].Value = "Prezime";
                worksheet.Cells[1, 3].Value = "Email";
                worksheet.Cells[1, 4].Value = "Mjesto Stanovanja";
                worksheet.Cells[1, 5].Value = "Broj telefona";
                worksheet.Cells[1, 6].Value = "URL";
                worksheet.Cells[1, 7].Value = "Nadređeni suradnik";
                worksheet.Cells[1, 8].Value = "Uloga";
                worksheet.Cells[1, 9].Value = "Posao";

                for (int i = 0; i < suradnik.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = suradnik[i].Ime;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment =
                        OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = suradnik[i].Prezime;
                    worksheet.Cells[i + 2, 3].Value = suradnik[i].Email;
                    worksheet.Cells[i + 2, 4].Value = suradnik[i].MjestoStanovanja;
                    worksheet.Cells[i + 2, 5].Value = suradnik[i].BrojTelefona;
                    worksheet.Cells[i + 2, 6].Value = suradnik[i].URL;
                    worksheet.Cells[i + 2, 7].Value = $"{suradnik[i].NadređeniEmailNavigation?.Ime ?? ""} {suradnik[i].NadređeniEmailNavigation?.Prezime ?? ""}".Trim() != "" ? $"{suradnik[i].NadređeniEmailNavigation?.Ime} {suradnik[i].NadređeniEmailNavigation?.Prezime}".Trim() : "-";


                    List<String> uloge =  suradnik[i].Uloga.Select(u => u.Ime).ToList();

                    StringBuilder builderic2 = new StringBuilder();

                    if (uloge.Count == 0)
                    {
                        worksheet.Cells[i + 2, 8].Value = "-";
                    }
                    else
                    {
                        foreach (var VARIABLE in uloge)
                        {
                            builderic2.Append(VARIABLE + ", ");
                        }

                        if (builderic2.ToString().Contains(", "))
                        {
                            builderic2.Remove(builderic2.Length - 2, 2);
                        }

                        worksheet.Cells[i + 2, 8].Value = builderic2;
                    }


                List<Posao> posloviZad = suradnik[i].Posao.ToList();
                        
                    StringBuilder builderic = new StringBuilder();

                    if (posloviZad.Count == 0)
                    {
                        worksheet.Cells[i + 2, 9].Value = "-";
                    }
                    else
                    {

                        foreach (var VARIABLE in posloviZad)
                        {
                            builderic.Append(VARIABLE.Opis + ", ");
                        }

                        if (builderic.ToString().Contains(", "))
                        {
                            builderic.Remove(builderic.Length - 2, 2);
                        }

                        worksheet.Cells[i + 2, 9].Value = builderic;
                    }
                }

                worksheet.Cells[1, 1, suradnik.Count + 1, 9].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "SuradniciMD.xlsx");
        }


        public async Task<IActionResult> Suradnici()
        {
            var suradnik = await _context.Suradnik.AsNoTracking()
                .Include(s => s.Uloga)
                .Include(s => s.Posao)
                .Include(s => s.Zadatak)
                .Include(s => s.NadređeniEmailNavigation)
                .Include(s => s.Projekt)
                .ToListAsync();

            byte[] content;

            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis suradnika";
                excel.Workbook.Properties.Author = "Marko Ćurković";
                var worksheet = excel.Workbook.Worksheets.Add("Suradnici");

                worksheet.Cells[1, 1].Value = "Ime";
                worksheet.Cells[1, 2].Value = "Prezime";
                worksheet.Cells[1, 3].Value = "Email";
                worksheet.Cells[1, 4].Value = "Mjesto Stanovanja";
                worksheet.Cells[1, 5].Value = "Broj telefona";
                worksheet.Cells[1, 6].Value = "URL";
                worksheet.Cells[1, 7].Value = "Nadređeni suradnik";
                worksheet.Cells[1, 8].Value = "Uloga";

                for (int i = 0; i < suradnik.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = suradnik[i].Ime;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment =
                        OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = suradnik[i].Prezime;
                    worksheet.Cells[i + 2, 3].Value = suradnik[i].Email;
                    worksheet.Cells[i + 2, 4].Value = suradnik[i].MjestoStanovanja;
                    worksheet.Cells[i + 2, 5].Value = suradnik[i].BrojTelefona;
                    worksheet.Cells[i + 2, 6].Value = suradnik[i].URL;
                    worksheet.Cells[i + 2, 7].Value = $"{suradnik[i].NadređeniEmailNavigation?.Ime ?? ""} {suradnik[i].NadređeniEmailNavigation?.Prezime ?? ""}".Trim() != "" ? $"{suradnik[i].NadređeniEmailNavigation?.Ime} {suradnik[i].NadređeniEmailNavigation?.Prezime}".Trim() : "-";


                    List<String> uloge = suradnik[i].Uloga.Select(u => u.Ime).ToList();

                    StringBuilder builderic2 = new StringBuilder();
                    if (uloge.Count == 0)
                    {
                        worksheet.Cells[i + 2, 8].Value = "-";
                    }
                    else
                    {
                        foreach (var VARIABLE in uloge)
                        {
                            builderic2.Append(VARIABLE + ", ");
                        }

                        if (builderic2.ToString().Contains(", "))
                        {
                            builderic2.Remove(builderic2.Length - 2, 2);
                        }

                        worksheet.Cells[i + 2, 8].Value = builderic2;
                    }

                }

                worksheet.Cells[1, 1, suradnik.Count + 1, 9].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "Suradnici.xlsx");
        }

        public async Task<IActionResult> Poslovi()
        {
            var poslovi = await _context.Posao.AsNoTracking()
                .Include(p => p.SuradnikEmailNavigation)
                .Include(p => p.VrstaPosla)
                .Include(p => p.Zadatak)
                .ToListAsync();

            byte[] content;

            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis poslova";
                excel.Workbook.Properties.Author = "Marko Ćurković";
                var worksheet = excel.Workbook.Worksheets.Add("Poslovi");

                worksheet.Cells[1, 1].Value = "Opis posla";
                worksheet.Cells[1, 2].Value = "Vrijeme početka rada";
                worksheet.Cells[1, 3].Value = "Vrijeme kraja rada";
                worksheet.Cells[1, 4].Value = "Vrsta posla";
                worksheet.Cells[1, 5].Value = "Zadatak";
                worksheet.Cells[1, 6].Value = "Suradnik";

                for (int i = 0; i < poslovi.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = poslovi[i].Opis;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment =
                        OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = poslovi[i].VrijemePočetkaRada.ToString("yyyy-MM-dd HH:mm");
                    worksheet.Cells[i + 2, 3].Value = poslovi[i].VrijemeKrajaRada.ToString("yyyy-MM-dd HH:mm");
                    worksheet.Cells[i + 2, 4].Value = poslovi[i].VrstaPosla.Ime;   
                    worksheet.Cells[i + 2, 5].Value = poslovi[i].Zadatak.OpisZadatka;
                    worksheet.Cells[i + 2, 6].Value = poslovi[i].SuradnikEmailNavigation.Ime + " " + poslovi[i].SuradnikEmailNavigation.Prezime;
                }

                worksheet.Cells[1, 1, poslovi.Count + 1, 6].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
               return File(content, ExcelContentType, "Poslovi.xlsx");
        }

        public async Task<IActionResult> SuradniciPDF()
        {
            string naslov = "Popis suradnika";
            var suradnik = await _context.Suradnik.AsNoTracking()
                .Include(s => s.Uloga)
                .Include(s => s.Posao)
                .Include(s => s.NadređeniEmailNavigation)
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

            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(suradnik));

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

                    column.PropertyName<Suradnik>(o => o.Ime);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(1);
                    column.HeaderCell("Ime", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Suradnik>(o => o.Prezime);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Prezime", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Suradnik>(o => o.Email);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(2);
                    column.HeaderCell("Email", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Suradnik>(o => o.MjestoStanovanja);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(2);
                    column.HeaderCell("Mjesto stanovanja", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Suradnik>(o => o.BrojTelefona);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4);
                    column.Width(2);
                    column.HeaderCell("Broj telefona", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Suradnik>(o => o.URL);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(5);
                    column.Width(2);
                    column.HeaderCell("URL", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Suradnik>(o => o.NadređeniEmail);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(6);
                    column.Width(2);
                    column.HeaderCell("Nadređeni suradnik", horizontalAlignment: HorizontalAlignment.Center);
                });
            });
            #endregion

            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=Suradnici.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }


        public async Task<IActionResult> PosloviPDF()
        {
            string naslov = "Popis poslova";

            var poslovi = await _context.Posao.AsNoTracking()
                .Include(p => p.SuradnikEmailNavigation)
                .Include(p => p.VrstaPosla)
                .Include(p => p.Zadatak)
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

            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(poslovi));

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
                    column.PropertyName<Posao>(o => o.Opis);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(1);
                    column.HeaderCell("Opis posla", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Posao>(o => o.VrijemePočetkaRada);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Vrijeme početka rada", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Posao>(o => o.VrijemeKrajaRada);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(2);
                    column.HeaderCell("Vrijeme kraja rada", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Posao>(o => o.VrstaPosla.Ime);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(2);
                    column.HeaderCell("Vrsta posla", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Posao>(o => o.Zadatak.OpisZadatka);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4);
                    column.Width(2);
                    column.HeaderCell("Zadatak", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Posao>(o => o.SuradnikEmail);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4);
                    column.Width(2);
                    column.HeaderCell("Zadatak", horizontalAlignment: HorizontalAlignment.Center);
                });
            });
            #endregion

            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=Poslovi.pdf");
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
                    Author = "Marko Ćurković",
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
        public async Task<IActionResult> UveziSuradnika(IFormFile excelFile)
        {
            List<Suradnik> suradnicinovi = new List<Suradnik>();
            List<Suradnik> uspjeh = new List<Suradnik>();
            List<Suradnik> neuspjeh = new List<Suradnik>();
            List<String> broj = new List<String>();

            if (excelFile == null || excelFile.Length == 0)
            {

                ModelState.AddModelError("excelFile", "Odaberite Excel datoteku.");
                return RedirectToAction("UvozIzvoz", "Suradnik");
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
                            string Prezime = worksheet.Cells[row, 2].Value.ToString();
                            string Email = worksheet.Cells[row, 3].Value.ToString();
                            string MjestoStanovanja = worksheet.Cells[row, 4].Value.ToString();
                            string BrojTelefona = worksheet.Cells[row, 5].Value.ToString();
                            string URL = worksheet.Cells[row, 6].Value.ToString();
                            string Nadređeni = worksheet.Cells[row, 7].Value.ToString();
                            string Uloga = worksheet.Cells[row, 8].Value.ToString();

                            List<String> uloge = Uloga.Split(',').ToList();

                            List<Uloga> uloge2 = new List<Uloga>();
                            uloge2 = _context.Uloga.Where(u => uloge.Contains(u.Ime)).ToList();

                            string broj1 = null;
                            if (Nadređeni != null && Nadređeni.Length > 3)
                            {
                                string[] dijelovi = Nadređeni.Split(' ');

                                broj1 = _context.Suradnik
                                    .Where(z => z.Ime == dijelovi[0] && z.Prezime == dijelovi[1])
                                    .Select(z => z.Email)
                                    .FirstOrDefault();
                            }

                            if (Ime != null && Prezime != null && Email != null && MjestoStanovanja != null && BrojTelefona != null)
                            {
                                Suradnik suradnik = new Suradnik
                                {
                                    Ime = Ime,
                                    Prezime = Prezime,
                                    Email = Email,
                                    MjestoStanovanja = MjestoStanovanja,
                                    BrojTelefona = BrojTelefona,
                                    URL = URL,
                                    NadređeniEmail = broj1
                                };
                                suradnik.NadređeniEmailNavigation = _context.Suradnik.Where(s => s.Email == broj1).FirstOrDefault();
                                suradnik.Uloga = uloge2;

                               var postoji = _context.Suradnik.Where(s => s.Email == suradnik.Email).FirstOrDefault();
                               if(postoji != null)
                                {
                                    broj.Add("Nije dodan");
                                }
                                else
                                {
                                    suradnicinovi.Add(suradnik);
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
                foreach (var VARIABLE in suradnicinovi)
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
                return RedirectToAction("UvozIzvoz", "Suradnik");
            }


        }


        [HttpPost]
        public async Task<IActionResult> UveziPosao(IFormFile excelFile2)
        {
            List<Posao> poslovinovi = new List<Posao>();
            List<Posao> uspjeh = new List<Posao>();
            List<Posao> neuspjeh = new List<Posao>();
            List<String> broj = new List<String>();

            if (excelFile2 == null || excelFile2.Length == 0)
            {

                ModelState.AddModelError("excelFile", "Odaberite Excel datoteku.");
                return RedirectToAction("UvozIzvoz", "Suradnik");
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
                            string opis = worksheet.Cells[row, 1].Value.ToString();
                            DateTime vrijemePočetkaRada = DateTime.Parse(worksheet.Cells[row, 2].Value.ToString());
                            DateTime vrijemeKrajaRada = DateTime.Parse(worksheet.Cells[row, 3].Value.ToString());
                            string vrstaPosla = worksheet.Cells[row, 4].Value.ToString();
                            string zadatak = worksheet.Cells[row, 5].Value.ToString();
                            string suradnik = worksheet.Cells[row, 6].Value.ToString();

                            string broj1 = null;
                            if (suradnik != null && suradnik.Length > 3)
                            {
                                string[] dijelovi = suradnik.Split(' ');

                                broj1 = _context.Suradnik
                                    .Where(z => z.Ime == dijelovi[0] && z.Prezime == dijelovi[1])
                                    .Select(z => z.Email)
                                    .FirstOrDefault();
                            }

                            VrstaPosla vrstaPosla1 = _context.VrstaPosla.Where(v => v.Ime == vrstaPosla).FirstOrDefault();
                            Zadatak zadatak1 = _context.Zadatak.Where(z => z.OpisZadatka == zadatak).FirstOrDefault();

                            if (opis != null && vrijemePočetkaRada.ToString().Length > 6 && vrijemeKrajaRada.ToString().Length > 6 && vrstaPosla != null && zadatak != null && suradnik != null)
                            {
                                
                                Posao posao = new Posao
                                {
                                    Opis = opis,
                                    VrijemePočetkaRada = vrijemePočetkaRada,
                                    VrijemeKrajaRada = vrijemeKrajaRada,
                                    VrstaPosla = vrstaPosla1,
                                    Zadatak = zadatak1,
                                    SuradnikEmail = broj1,
                                    VrstaPoslaId = vrstaPosla1.VrstaPoslaId,
                                    ZadatakId = zadatak1.ZadatakId,
                                    SuradnikEmailNavigation = _context.Suradnik.Where(s => s.Email == broj1).FirstOrDefault()
                                };
                                    poslovinovi.Add(posao);
                                    broj.Add("Dodan");    
                            }
                            else
                            {
                                broj.Add("Nije dodan");
                            }

                        }
                    }
                }

                foreach (var VARIABLE in poslovinovi)
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
            catch(Exception e)
            {
                TempData["errorMessage"] = $"Dogodila se greška!";
                return RedirectToAction("UvozIzvoz", "Suradnik");
            }
        }

    }

}
