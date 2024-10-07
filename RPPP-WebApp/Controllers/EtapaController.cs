using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NuGet.Versioning;
using RPPP_WebApp.Data;
using RPPP_WebApp.Exstensions.Selectors;
using RPPP_WebApp.Models;
using RPPP_WebApp.ModelsValidation;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers
{

    public class EtapaController : Controller
    {
        private readonly RPPP06Context _context;
        private readonly AppSettings appSettings;
        private readonly ILogger<EtapaController> logger;
        EtapaValidator validator = new EtapaValidator();


        public EtapaController(RPPP06Context context, IOptionsSnapshot<AppSettings> options, ILogger<EtapaController> logger)
        {
            _context = context;
            appSettings = options.Value;
            this.logger = logger;
        }

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pageSize = appSettings.PageSize;

            var etapa = _context.Etapa.AsNoTracking();

            int count = etapa.Count();

            if (count == 0)
            {
                TempData[Constants.Message] = "Ne postoji niti jedna Etapa.";
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

            etapa = etapa
            .Include(z => z.PlanProjekta)
            .Include(z => z.Aktivnost);

            etapa = etapa.ApplySort(sort, ascending);

            var etapaSorted = etapa
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new EtapaPageView
            {
                Etapa = etapaSorted,
                PagingInfo = pagingInfo
            };

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareEtapaProps();
            Etapa etapa = new Etapa();
            var viewModel = new EtapaViewModel
            {
                Etapa = etapa,
            };
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int EtapaId)
        {
            await PrepareEtapaProps();
            Etapa etapa = new Etapa();
            etapa = _context.Etapa.Find(EtapaId);
            var viewModel = new EtapaViewModel
            {
                Etapa = etapa
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(EtapaViewModel viewModel)
        {
            var result = validator.Validate(viewModel.Etapa);
            if (result.IsValid)
            {
                var existingEtapa = _context.Etapa
                    .Where(z => z.Ime == viewModel.Etapa.Ime)
                    .FirstOrDefault();

                if (existingEtapa != null)
                {
                    TempData["errorMessage"] = $"Etapu nije moguće dodati jer etapa s istim imenom već postoji.";
                    Task.Run(async () => await PrepareEtapaProps()).Wait();
                    return View(viewModel);
                }

                _context.Etapa.Add(viewModel.Etapa);
                _context.SaveChanges();

                TempData[Constants.ErrorOccurred] = false;
                TempData["SuccessMessage"] = $"Etapa {viewModel.Etapa.EtapaId} dodana.";
                return RedirectToAction(nameof(Index));
            }
            TempData["errorMessage"] = $"Etapu nije moguće dodati.";
            Task.Run(async () => await PrepareEtapaProps()).Wait();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EtapaViewModel viewModel, int EtapaId)
        {
            var result = validator.Validate(viewModel.Etapa);
            if (result.IsValid)
            {
                var etapa = _context.Etapa.Find(EtapaId);

                etapa.Ime = viewModel.Etapa.Ime;
                etapa.Opis = viewModel.Etapa.Opis;
                etapa.PlanProjektaId = viewModel.Etapa.PlanProjektaId;
                etapa.AktivnostId = viewModel.Etapa.AktivnostId;

                _context.Etapa.Update(etapa);
                _context.SaveChanges();

                TempData[Constants.ErrorOccurred] = false;
                TempData["SuccessMessage"] = $"Izmjene nad etapom {EtapaId} su spremljene.";
                return RedirectToAction(nameof(Index));
            }
            TempData["errorMessage"] = $"Izmjene nad etapom {EtapaId} nije moguće spremiti.";
            Task.Run(async () => await PrepareEtapaProps()).Wait();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int EtapaId, int page = 1, int sort = 1, bool ascending = true)
        {
            var etapaToBeDeleted = _context.Etapa.Find(EtapaId);
            if (etapaToBeDeleted != null)
            {
                try
                {
                    _context.Remove(etapaToBeDeleted);
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = $"Etapa {etapaToBeDeleted.EtapaId} uspješno obrisana.";
                }
                catch (Exception ex)
                {
                    TempData["errorMessage"] = $"Etapa {etapaToBeDeleted.EtapaId} se ne može obrisati.";
                }
            }
            else
            {
                TempData["errorMessage"] = $"Etapa {etapaToBeDeleted.EtapaId} se ne može obrisati.";
            }


            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
        }


        private async Task PrepareEtapaProps()
        {
            await PrepareAktivnostDropdownList();
            await PreparePlanProjektaDropwdownList();
        }

        private async Task PrepareAktivnostDropdownList()
        {
            var aktivnosti = await _context.Aktivnost
                            .OrderBy(t => t.AktivnostId)
                            .Select(d => new SelectListItem
                            {
                                Value = d.AktivnostId.ToString(),
                                Text = d.Ime
                            }).ToListAsync();

            ViewBag.Aktivnosti = new SelectList(aktivnosti, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
        }

        private async Task PreparePlanProjektaDropwdownList()
        {
            var planoviProjekta = await _context.PlanProjekta
                            .OrderBy(t => t.PlanProjektaId)
                            .Select(d => new SelectListItem
                            {
                                Value = d.PlanProjektaId.ToString(),
                                Text = d.PlanProjektaId.ToString()
                            }).ToListAsync();

            ViewBag.PlanoviProjekta = new SelectList(planoviProjekta, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create2(int planProjektId, string etapaIme, string etapaOpis, int aktivnostId, string aktivnostIme)
        {
            var novaEtapa = new Etapa
            {
                Ime = etapaIme,
                Opis = etapaOpis,
                PlanProjektaId = planProjektId,
                AktivnostId = aktivnostId
            };

            var result = validator.Validate(novaEtapa);
            if (result.IsValid)
            {
                var existingEtapa = _context.Etapa
                    .Where(z => z.Ime == etapaIme)
                    .FirstOrDefault();

                if (existingEtapa != null)
                {
                    TempData["errorMessage"] = $"Etapu nije moguće dodati jer etapa s istim imenom već postoji.";
                    return RedirectToAction("MasterDetailEdit", "PlanProjekta", new { PlanProjektaId = planProjektId });
                }

                _context.Etapa.Add(novaEtapa);
                _context.SaveChanges();

                TempData[Constants.ErrorOccurred] = false;
                TempData["SuccessMessage"] = $"Etapa dodana.";
                return RedirectToAction("MasterDetailEdit", "PlanProjekta", new { PlanProjektaId = planProjektId });
            }
            else
            {
                TempData["errorMessage"] = $"Etapu nije moguće dodati.";
                return RedirectToAction("MasterDetailEdit", "PlanProjekta", new { PlanProjektaId = planProjektId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete2(int EtapaId, int PlanProjektaId)
        {
            var etapaToBeDeleted = _context.Etapa.Find(EtapaId);
            if (etapaToBeDeleted != null)
            {
                try
                {
                    _context.Remove(etapaToBeDeleted);
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = $"Etapa {etapaToBeDeleted.EtapaId} uspješno obrisana.";
                }
                catch (Exception ex)
                {
                    TempData["errorMessage"] = $"Etapa {etapaToBeDeleted.EtapaId} se ne može obrisati.";
                }
            }
            else
            {
                TempData["errorMessage"] = $"Etapa {etapaToBeDeleted.EtapaId} se ne može obrisati.";
            }

            return RedirectToAction("MasterDetailEdit", "PlanProjekta", new { PlanProjektaId = PlanProjektaId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit2(PlanProjektaViewModel viewModel, int EtapaId, int PlanProjektaId)
        {
            var newEtapa = viewModel.Etape.FirstOrDefault(e => e.EtapaId == EtapaId);

            var aktivnost = _context.Aktivnost.Find(newEtapa.AktivnostId);
            newEtapa.Aktivnost = aktivnost;

            var planProjekta = _context.PlanProjekta.Find(PlanProjektaId);
            newEtapa.PlanProjektaId = PlanProjektaId;
            newEtapa.PlanProjekta = planProjekta;

            _context.Etapa.Update(newEtapa);
            _context.SaveChanges();

            TempData[Constants.ErrorOccurred] = false;
            TempData["SuccessMessage"] = $"Izmjene nad etapom {EtapaId} su spremljene.";

            //TempData["errorMessage"] = $"Izmjene nad etapom {EtapaId} nije moguće spremiti.";

            return RedirectToAction("MasterDetailEdit", "PlanProjekta", new
            {
                PlanProjektaId = PlanProjektaId
            });
        }
    }
}
