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

    public class ZadatakController : Controller
    {
        private readonly RPPP06Context _context;
        private readonly AppSettings appSettings;
        private readonly ILogger<ZadatakController> logger;
        ZadatakValidator validator = new ZadatakValidator();


        public ZadatakController(RPPP06Context context, IOptionsSnapshot<AppSettings> options, ILogger<ZadatakController> logger)
        {
            _context = context;
            appSettings = options.Value;
            this.logger = logger;
        }

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pageSize = appSettings.PageSize;

            var zadatak = _context.Zadatak.AsNoTracking();

            int count = zadatak.Count();

            if (count == 0)
            {
                TempData[Constants.Message] = "Ne postoji niti jedan Zadatak.";
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

            zadatak = zadatak
            .Include(z => z.Zahtjev)
            .Include(z => z.Status);

            zadatak = zadatak.ApplySort(sort, ascending);

            var zadatciSorted = zadatak
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new ZadatakPageView
            {
                Zadatak = zadatciSorted,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int page = 1, int sort = 1, bool ascending = true)
        {
            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending
            };

            await PrepareZadatakProps();
            Zadatak Zadatak = new Zadatak();
            var viewModel = new ZadatakViewModel
            {
                Zadatak = Zadatak,
                PagingInfo = pagingInfo
            };
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int ZadatakId, int page = 1, int sort = 1, bool ascending = true)
        {
            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending
            };

            await PrepareZadatakProps();
            Zadatak Zadatak = new Zadatak();
            Zadatak = _context.Zadatak.Find(ZadatakId);
            var viewModel = new ZadatakViewModel
            {
                Zadatak = Zadatak,
                PagingInfo = pagingInfo
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ZadatakViewModel viewModel, int page = 1, int sort = 1, bool ascending = true)
        {
            var result = validator.Validate(viewModel.Zadatak);
            if (result.IsValid)
            {
                _context.Zadatak.Add(viewModel.Zadatak);

                _context.SaveChanges();
                TempData[Constants.ErrorOccurred] = false;
                TempData["SuccessMessage"] = $"Suradnik {viewModel.Zadatak.ZadatakId} dodan.";
                return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
            }
            TempData["errorMessage"] = $"Zadatak nije moguće dodati.";
            Task.Run(async () => await PrepareZadatakProps()).Wait();
            return View(nameof(Create), new { page = page, sort = sort, ascending = ascending });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ZadatakViewModel viewModel, int ZadatakId, int page = 1, int sort = 1, bool ascending = true)
        {
            var result = validator.Validate(viewModel.Zadatak);
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
                return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
            }
            TempData["errorMessage"] = $"Zadatak nije moguće promijeniti.";
            Task.Run(async () => await PrepareZadatakProps()).Wait();
            return View(nameof(Edit), new { ZadatakId = ZadatakId, page = page, sort = sort, ascending = ascending });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int ZadatakId, int? ZahtjevId, string ViewType, int page = 1, int sort = 1, bool ascending = true)
        {
            var ZadatakToBeDeleted = _context.Zadatak.Find(ZadatakId);
            if (ZadatakToBeDeleted != null)
            {
                try
                {
                    _context.Remove(ZadatakToBeDeleted);
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = $"Zadatak {ZadatakToBeDeleted.ZadatakId} uspješno obrisan.";
                }
                catch (Exception)
                {
                    TempData["errorMessage"] = $"Zadatak {ZadatakToBeDeleted.ZadatakId} se ne može obrisati.";
                }
            }
            else
            {
                TempData["errorMessage"] = $"Zadatak {ZadatakToBeDeleted.ZadatakId} se ne može obrisati.";
            }
                if (ViewType == "ShowEdit")
                {
                    return RedirectToAction("ShowEdit", "Zahtjev", new { ZahtjevId= ZahtjevId, page = page, sort = sort, ascending = ascending });
                }
                else 
                {
                    return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
                }

        }

        private async Task PrepareZadatakProps()
        {
            await PrepareStatusDropdownList();
            await PrepareNositeljEmailDropwdownList();
            await PrepareZahtjevDropdownList();
        }

        private async Task PrepareStatusDropdownList()
        {
            var Statusi = await _context.Status
                            .OrderBy(t => t.StatusId)
                            .Select(d => new SelectListItem
                            {
                                Value = d.StatusId.ToString(),
                                Text = d.Ime
                            }).ToListAsync();

            ViewBag.Statusi = new SelectList(Statusi, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
        }

        private async Task PrepareNositeljEmailDropwdownList()
        {
            var Suradnici = await _context.Suradnik
                            .OrderBy(t => t.Email)
                            .Select(d => new SelectListItem
                            {
                                Value = d.Email,
                                Text = d.Email
                            }).ToListAsync();

            ViewBag.Suradnici = new SelectList(Suradnici, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
        }

        private async Task PrepareZahtjevDropdownList()
        {
            var Zahtjevi = await _context.Zahtjev
                            .OrderBy(t => t.ZahtjevId)
                            .Select(d => new SelectListItem
                            {
                                Value = d.ZahtjevId.ToString(),
                                Text = d.Ime
                            }).ToListAsync();

            ViewBag.Zahtjevi = new SelectList(Zahtjevi, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
        }
    }


}
