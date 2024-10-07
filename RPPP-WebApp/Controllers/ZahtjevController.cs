using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using RPPP_WebApp.Data;
using RPPP_WebApp.Exstensions.Selectors;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.ModelsValidation;
using System.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using NLog.Fluent;

namespace RPPP_WebApp.Controllers
{

    public class ZahtjevController : Controller
    {
        private readonly RPPP06Context _context;
        private readonly AppSettings appSettings;
        private readonly ILogger<ZahtjevController> logger;
        ZahtjevValidator validator = new ZahtjevValidator();

        ZadatakValidator validatorZadatak = new ZadatakValidator();


        public ZahtjevController(RPPP06Context context, IOptionsSnapshot<AppSettings> options, ILogger<ZahtjevController> logger)
        {
            _context = context;
            appSettings = options.Value;
            this.logger = logger;
        }

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pageSize = appSettings.PageSize;

            var zahtjevi = _context.Zahtjev.AsNoTracking();

            int count = zahtjevi.Count();

            if (count == 0)
            {
                TempData[Constants.Message] = "Ne postoji niti jedan Zahtjev.";
                TempData[Constants.ErrorOccurred] = false;
                return RedirectToAction(nameof(Create));
            }

            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending,
                ItemsPerPage = pageSize,
                TotalItems = count
            };

            if (page < 1 || page > pagingInfo.TotalPages)
            {
                return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
            }

            zahtjevi = zahtjevi
            .Include(z => z.PlanProjekta)
                .ThenInclude(pp => pp.Projekt)
            .Include(z => z.Prioritet)
            .Include(z => z.TipZahtjeva)
            .Include(z => z.Zadatak);

            zahtjevi = zahtjevi.ApplySort(sort, ascending);

            var zahtjeviSorted = zahtjevi
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new ZahtjevPageView
            {
                Zahtjevi = zahtjeviSorted,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        public IActionResult Master(string ViewType, int page = 1, int sort = 1, bool ascending = true)
        {
            int pageSize = appSettings.PageSize;

            var zahtjevi = _context.Zahtjev.AsNoTracking();

            int count = zahtjevi.Count();

            if (count == 0)
            {
                TempData[Constants.Message] = "Ne postoji niti jedan Zahtjev.";
                TempData[Constants.ErrorOccurred] = false;
                return RedirectToAction(nameof(Create));
            }

            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending,
                ItemsPerPage = pageSize,
                TotalItems = count
            };

            if (page < 1 || page > pagingInfo.TotalPages)
            {
                return RedirectToAction(ViewType, new { page = 1, sort, ascending });
            }

            zahtjevi = zahtjevi
            .Include(z => z.PlanProjekta)
                .ThenInclude(pp => pp.Projekt)
            .Include(z => z.Prioritet)
            .Include(z => z.TipZahtjeva)
            .Include(z => z.Zadatak);


            zahtjevi = zahtjevi.ApplySort(sort, ascending);

            var zahtjeviSorted = zahtjevi
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

            var model = new ZahtjevPageView
            {
                Zahtjevi = zahtjeviSorted,
                PagingInfo = pagingInfo
            };


            return View(ViewType, model);
        }

        public IActionResult Details(int? id, int page = 1, int sort = 1, bool ascending = true)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending
            };

            var zahtjev = _context.Zahtjev
                .Include(z => z.PlanProjekta)
                .Include(z => z.Prioritet)
                .Include(z => z.TipZahtjeva)
                .Include(z => z.Zadatak)
                    .ThenInclude(z => z.Status)
                .FirstOrDefault(m => m.ZahtjevId == id);

            if (zahtjev == null)
            {
                return NotFound();
            }

            var model = new ZahtjevViewModel
            {
                Zahtjev = zahtjev,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        public IActionResult ShowEdit(int ZahtjevId, int? position, int page = 1, int sort = 1, bool ascending = true)
        {
            var zahtjev = _context.Zahtjev
                .Include(z => z.PlanProjekta)
                    .ThenInclude(pp => pp.Projekt)
                .Include(z => z.Prioritet)
                .Include(z => z.TipZahtjeva)
                .Include(z => z.Zadatak)
                    .ThenInclude(z => z.Status)
                .FirstOrDefault(m => m.ZahtjevId == ZahtjevId);

            if (zahtjev == null)
            {
                return NotFound();
            }

            var model = new ZahtjevZadatakViewModel
            {
                Zahtjev = zahtjev,
                Zadatak = new Zadatak(),
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    Sort = sort,
                    Ascending = ascending
                }
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateZadatak(ZahtjevZadatakViewModel viewModel, int ZahtjevId, int page = 1, int sort = 1, bool ascending = true)
        {
            var result = validatorZadatak.Validate(viewModel.Zadatak);

            if (result.IsValid)
            {
                var status = _context.Status.Find(viewModel.Zadatak.StatusId);
                viewModel.Zadatak.Status = status;
                _context.Zadatak.Add(viewModel.Zadatak);

                _context.SaveChanges();
                TempData[Constants.ErrorOccurred] = false;
                TempData["SuccessMessage"] = $"Zadatak {viewModel.Zadatak.ZadatakId} dodan.";
                return RedirectToAction("ShowEdit", new { ZahtjevId = ZahtjevId, page = page, sort = sort, ascending = ascending });
            }
            TempData["errorMessage"] = $"Zadatak nije moguće dodati.";
            return RedirectToAction("ShowEdit", new { ZahtjevId = ZahtjevId, page = page, sort = sort, ascending = ascending });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ShowEdit(ZahtjevViewModel viewModel, int ZahtjevId, int page = 1, int sort = 1, bool ascending = true)
        {
            var result = validator.Validate(viewModel.Zahtjev);
            if (result.IsValid)
            {
                var zahtjev = _context.Zahtjev.Find(ZahtjevId);
                zahtjev.Ime = viewModel.Zahtjev.Ime;
                zahtjev.Opis = viewModel.Zahtjev.Opis;
                zahtjev.PlanProjektaId = viewModel.Zahtjev.PlanProjektaId;
                zahtjev.PrioritetId = viewModel.Zahtjev.PrioritetId;
                zahtjev.TipZahtjevaId = viewModel.Zahtjev.TipZahtjevaId;
                _context.Zahtjev.Update(zahtjev);

                _context.SaveChanges();

                TempData[Constants.ErrorOccurred] = false;
                TempData["SuccessMessage"] = $"Zahtjev {ZahtjevId} promijenjen.";
                return RedirectToAction("ShowEdit", new { ZahtjevId = ZahtjevId, page = page, sort = sort, ascending = ascending });
            }
            TempData[Constants.ErrorOccurred] = true;
            TempData["errorMessage"] = $"Zahtjev {viewModel.Zahtjev.ZahtjevId} nije moguće promijeniti.";
            return RedirectToAction("ShowEdit", new { ZahtjevId = ZahtjevId, page = page, sort = sort, ascending = ascending });
        }


        private async Task SetPreviousAndNext(int position, int sort, bool ascending)
        {
            var query = _context.Zahtjev.AsQueryable();


            query = query.ApplySort(sort, ascending);
            if (position > 0)
            {
                ViewBag.Previous = await query.Skip(position - 1).Select(d => d.ZahtjevId).FirstAsync();
            }
            if (position < await query.CountAsync() - 1)
            {
                ViewBag.Next = await query.Skip(position + 1).Select(d => d.ZahtjevId).FirstAsync();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create(string ViewType, int page = 1, int sort = 1, bool ascending = true)
        {
            int pageSize = appSettings.PageSize;

            var zahtjevi = _context.Zahtjev.AsNoTracking();

            int count = zahtjevi.Count();

            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending
            };

            await PrepareZahtjevProps();
            Zahtjev zahtjev = new Zahtjev();
            var viewModel = new ZahtjevViewModel
            {
                Zahtjev = zahtjev,
                PagingInfo = pagingInfo
            };

            ViewBag.ViewType = ViewType;
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int ZahtjevId, int page = 1, int sort = 1, bool ascending = true)
        {
            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending
            };

            await PrepareZahtjevProps();
            Zahtjev zahtjev = new Zahtjev();
            zahtjev = _context.Zahtjev.Find(ZahtjevId);
            var viewModel = new ZahtjevViewModel
            {
                Zahtjev = zahtjev,
                PagingInfo = pagingInfo
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ZahtjevViewModel viewModel, string ViewType, int page = 1, int sort = 1, bool ascending = true)
        {
            var result = validator.Validate(viewModel.Zahtjev);
            if (result.IsValid)
            {
                _context.Zahtjev.Add(viewModel.Zahtjev);

                _context.SaveChanges();

                TempData[Constants.ErrorOccurred] = false;
                TempData["SuccessMessage"] = $"Zahtjev {viewModel.Zahtjev.ZahtjevId} dodan.";
                if (ViewType == "Index")
                {
                    return RedirectToAction("Index", new { ViewType = "Index", page = page, sort = sort, ascending = ascending });
                }
                else if (ViewType == "Show")
                {
                    return RedirectToAction("Master", new { ViewType = "Show", page = page, sort = sort, ascending = ascending });
                }
            }
            TempData["errorMessage"] = $"Zahtjev nije moguće dodati.";
            Task.Run(async () => await PrepareZahtjevProps()).Wait();
            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditZadatak(ZahtjevZadatakViewModel viewModel, int ZadatakId, int ZahtjevId, int page = 1, int sort = 1, bool ascending = true)
        {
            var result = validatorZadatak.Validate(viewModel.Zadatak);
            if (result.IsValid)
            {

                var Zadatak = _context.Zadatak.Find(ZadatakId);
                Zadatak.OpisZadatka = viewModel.Zadatak.OpisZadatka;
                Zadatak.PlaniraniPočetak = viewModel.Zadatak.PlaniraniPočetak;
                Zadatak.PlaniraniKraj = viewModel.Zadatak.PlaniraniKraj;
                Zadatak.StvarniPočetak = viewModel.Zadatak.StvarniPočetak;
                Zadatak.StvarniKraj = viewModel.Zadatak.StvarniKraj;
                Zadatak.StvarniPočetak = viewModel.Zadatak.StvarniPočetak;
                Zadatak.NositeljEmail = viewModel.Zadatak.NositeljEmail;
                Zadatak.StatusId = viewModel.Zadatak.StatusId;
                Zadatak.ZahtjevId = viewModel.Zadatak.ZahtjevId;
                _context.Zadatak.Update(Zadatak);

                _context.SaveChanges();
                TempData[Constants.ErrorOccurred] = false;
                TempData["SuccessMessage"] = $"Zadatak {ZadatakId} promijenjen.";
                return RedirectToAction("ShowEdit", new { ZahtjevId = ZahtjevId, page = page, sort = sort, ascending = ascending });
            }
            TempData["errorMessage"] = $"Zadatak nije moguće promijeniti.";
            return RedirectToAction("ShowEdit", new { ZahtjevId = ZahtjevId, page = page, sort = sort, ascending = ascending });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ZahtjevViewModel viewModel, int ZahtjevId, int page = 1, int sort = 1, bool ascending = true)
        {

            var result = validator.Validate(viewModel.Zahtjev);
            if (result.IsValid)
            {
                var zahtjev = _context.Zahtjev.Find(ZahtjevId);
                zahtjev.Ime = viewModel.Zahtjev.Ime;
                zahtjev.Opis = viewModel.Zahtjev.Opis;
                zahtjev.PlanProjektaId = viewModel.Zahtjev.PlanProjektaId;
                zahtjev.PrioritetId = viewModel.Zahtjev.PrioritetId;
                zahtjev.TipZahtjevaId = viewModel.Zahtjev.TipZahtjevaId;
                _context.Zahtjev.Update(zahtjev);

                _context.SaveChanges();

                TempData[Constants.ErrorOccurred] = false;
                TempData["SuccessMessage"] = $"Zahtjev {ZahtjevId} promijenjen.";
                return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
            }
            TempData[Constants.ErrorOccurred] = true;
            TempData["errorMessage"] = $"Zahtjev {viewModel.Zahtjev.ZahtjevId} nije moguće promijeniti.";
            Task.Run(async () => await PrepareZahtjevProps()).Wait();
            return View(nameof(Edit), new { ZahtjevId = ZahtjevId, page = page, sort = sort, ascending = ascending });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int ZahtjevId, string ViewType, int page = 1, int sort = 1, bool ascending = true)
        {
            var zahtjevToBeDeleted = _context.Zahtjev.Find(ZahtjevId);
            if (zahtjevToBeDeleted != null)
            {
                try
                {
                    _context.Remove(zahtjevToBeDeleted);
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = $"Zahtjev {zahtjevToBeDeleted.ZahtjevId} uspješno obrisan.";
                }
                catch (Exception ex)
                {
                    TempData["errorMessage"] = $"Zahtjev {zahtjevToBeDeleted.ZahtjevId} se ne može obrisati.";
                }
            }
            else
            {
                TempData["errorMessage"] = $"Zahtjev {zahtjevToBeDeleted.ZahtjevId} se ne može obrisati.";
            }
            if (ViewType == "Index")
            {
                return RedirectToAction(ViewType, new { page = page, sort = sort, ascending = ascending });
            }
            else
            {
                return RedirectToAction(ViewType, new { ViewType = "Show", page = page, sort = sort, ascending = ascending });
            }
        }

        private async Task PrepareZahtjevProps()
        {
            await PrepareProjektDropDownList();
            await PreparePrioritetiDropDownList();
            await PrepareTipZahtjevaDropDownList();
        }

        private async Task PrepareProjektDropDownList()
        {
            var PlanoviProjekta = await _context.PlanProjekta
                        .OrderBy(t => t.ProjektId)
                        .Select(d => new SelectListItem
                        {
                            Value = d.PlanProjektaId.ToString(),
                            Text = d.Projekt.Ime.ToString()

                        }).ToListAsync();

            ViewBag.PlanProjekta = new SelectList(PlanoviProjekta, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
        }

        private async Task PrepareTipZahtjevaDropDownList()
        {
            var TipoviZahtjeva = await _context.TipZahtjeva
                        .OrderBy(t => t.TipZahtjevaId)
                        .Select(d => new SelectListItem
                        {
                            Value = d.TipZahtjevaId.ToString(),
                            Text = d.Ime
                        }).ToListAsync();

            ViewBag.TipZahtjeva = new SelectList(TipoviZahtjeva, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
        }

        private async Task PreparePrioritetiDropDownList()
        {
            var Prioriteti = await _context.Prioritet
                        .OrderBy(t => t.PrioritetId)
                        .Select(d => new SelectListItem
                        {
                            Value = d.PrioritetId.ToString(),
                            Text = d.Ime
                        }).ToListAsync();

            ViewBag.Prioritet = new SelectList(Prioriteti, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
        }

        [HttpGet]
        public IActionResult UvozIzvoz()
        {
            return View();
        }
    }
}
