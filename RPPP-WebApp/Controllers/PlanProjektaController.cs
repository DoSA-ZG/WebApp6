using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Data;
using RPPP_WebApp.Exstensions.Selectors;
using RPPP_WebApp.Models;
using RPPP_WebApp.ModelsValidation;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers
{
    public class PlanProjektaController : Controller
    {
        private readonly RPPP06Context _context;
        private readonly AppSettings appSettings;
        private readonly ILogger<PlanProjektaController> logger;
        PlanProjektaValidator validator = new PlanProjektaValidator();

        public PlanProjektaController(RPPP06Context context, IOptionsSnapshot<AppSettings> options, ILogger<PlanProjektaController> logger)
        {
            _context = context;
            appSettings = options.Value;
            this.logger = logger;
        }

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pageSize = appSettings.PageSize;

            var planovi = _context.PlanProjekta.AsNoTracking();

            int count = planovi.Count();

            if (count == 0)
            {
                TempData[Constants.Message] = "Ne postoji niti jedan plan projekta.";
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

            planovi = planovi
            .Include(p => p.Projekt)
            .Include(p => p.Etapa)
            .Include(p => p.Zahtjev)
            .Include(p => p.VoditeljEmailNavigation);

            planovi = planovi.ApplySort(sort, ascending);

            var planoviSorted = planovi
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new PlanProjektaPageView
            {
                Planovi = planoviSorted,
                PagingInfo = pagingInfo
            };

            return View(model);
        }


        public IActionResult Master(int page = 1, int sort = 1, bool ascending = true)
        {
            int pageSize = appSettings.PageSize;

            var planovi = _context.PlanProjekta.AsNoTracking();

            int count = planovi.Count();

            if (count == 0)
            {
                TempData[Constants.Message] = "Ne postoji niti jedan Plan projekta.";
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
                return RedirectToAction(nameof(Master), new { page = 1, sort, ascending });
            }

            planovi = planovi
            .Include(z => z.Projekt)
            .Include(z => z.Etapa)
            .Include(z => z.Zahtjev)
            .Include(z => z.VoditeljEmailNavigation);


            planovi = planovi.ApplySort(sort, ascending);

            var planoviSorted = planovi
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

            var model = new PlanProjektaPageView
            {
                Planovi = planoviSorted,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        public IActionResult Master2(int page = 1, int sort = 1, bool ascending = true)
        {
            return Master(page, sort, ascending);
        }


        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var planProjekta = _context.PlanProjekta.AsNoTracking()
                .Include(z => z.Etapa)
                    .ThenInclude(z => z.Aktivnost)
                .Include(z => z.Projekt)
                .Include(z => z.Zahtjev)
                .FirstOrDefault(m => m.PlanProjektaId == id);

            if (planProjekta == null)
            {
                return NotFound();
            }

            return View(planProjekta);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await preparePlanProjektaProps();
            PlanProjekta plan = new PlanProjekta();

            var viewModel = new PlanProjektaViewModel
            {
                PlanProjekta = plan,
            };
            return View(viewModel);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int PlanProjektaId)
        {
            await preparePlanProjektaProps();
            PlanProjekta planProjekta = new PlanProjekta();
            planProjekta = _context.PlanProjekta.Find(PlanProjektaId);

            var viewModel = new PlanProjektaViewModel
            {
                PlanProjekta = planProjekta,
            };
            return View(viewModel);
        }

        public Task<IActionResult> Edit2(int PlanProjektaId)
        {
            return Edit(PlanProjektaId);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PlanProjektaViewModel viewModel)
        {
            var result = validator.Validate(viewModel.PlanProjekta);
            if (result.IsValid)
            {
                _context.PlanProjekta.Add(viewModel.PlanProjekta);

                _context.SaveChanges();

                TempData[Constants.ErrorOccurred] = false;
                TempData["SuccessMessage"] = $"Plan projekta {viewModel.PlanProjekta.PlanProjektaId} dodan.";
                return RedirectToAction(nameof(Index));
            }
            TempData["errorMessage"] = $"Plan projekta nije moguće dodati.";
            Task.Run(async () => await preparePlanProjektaProps()).Wait();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(PlanProjektaViewModel viewModel, int PlanProjektaId)
        {
            var result = validator.Validate(viewModel.PlanProjekta);
            if (result.IsValid)
            {
                var plan = _context.PlanProjekta.Find(PlanProjektaId);
                plan.PlaniraniPočetak = viewModel.PlanProjekta.PlaniraniPočetak;
                plan.PlaniraniKraj = viewModel.PlanProjekta.PlaniraniKraj;
                plan.StvarniPočetak = viewModel.PlanProjekta.StvarniPočetak;
                plan.StvarniKraj = viewModel.PlanProjekta.StvarniKraj;
                plan.ProjektId = viewModel.PlanProjekta.ProjektId;
                plan.VoditeljEmail = viewModel.PlanProjekta.VoditeljEmail;

                _context.PlanProjekta.Update(plan);
                _context.SaveChanges();

                TempData[Constants.ErrorOccurred] = false;
                TempData["SuccessMessage"] = $"Izmjene nad plan projekta {PlanProjektaId} spremljene.";
                return RedirectToAction(nameof(Index));
            }

            TempData[Constants.ErrorOccurred] = true;
            TempData["errorMessage"] = $"Nije moguće spremiti izmjene nad Planom projekta {viewModel.PlanProjekta.PlanProjektaId}.";
            Task.Run(async () => await preparePlanProjektaProps()).Wait();

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int PlanProjektaId, int page = 1, int sort = 1, bool ascending = true)
        {
            var planToBeDeleted = _context.PlanProjekta.Find(PlanProjektaId);
            if (planToBeDeleted != null)
            {
                try
                {
                    _context.Remove(planToBeDeleted);
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = $"Plan projekta {planToBeDeleted.PlanProjektaId} uspješno obrisan.";
                }
                catch (Exception ex)
                {
                    TempData["errorMessage"] = $"Plan projekta {planToBeDeleted.PlanProjektaId} se ne može obrisati.";
                }
            }
            else
            {
                TempData["errorMessage"] = $"Plan projekta {planToBeDeleted.PlanProjektaId} se ne može obrisati.";
            }

            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
        }


        private async Task preparePlanProjektaProps()
        {
            await PrepareProjektDropDownList();
            await PrepareSuradnikDropDownList();
        }


        private async Task PrepareProjektDropDownList()
        {
            var projekti = await _context.Projekt
                        .OrderBy(t => t.ProjektId)
                        .Select(d => new SelectListItem
                        {
                            Value = d.ProjektId.ToString(),
                            Text = d.ProjektId.ToString()

                        }).ToListAsync();

            ViewBag.Projekti = new SelectList(projekti, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
        }


        private async Task PrepareSuradnikDropDownList()
        {
            var suradnici = await _context.Suradnik
                        .OrderBy(t => t.Email)
                        .Select(d => new SelectListItem
                        {
                            Value = d.Email.ToString(),
                            Text = $"{d.Ime} {d.Prezime}"
                        }).ToListAsync();

            ViewBag.Suradnici = new SelectList(suradnici, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
        }


        // 2. predaja
        public IActionResult Index2(int page = 1, int sort = 1, bool ascending = true)
        {
            int pageSize = appSettings.PageSize;

            var planovi = _context.PlanProjekta.AsNoTracking();

            int count = planovi.Count();

            if (count == 0)
            {
                TempData[Constants.Message] = "Ne postoji niti jedan plan projekta.";
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
                return RedirectToAction(nameof(Index2), new { page = 1, sort, ascending });
            }

            planovi = planovi
            .Include(p => p.Projekt)
            .Include(p => p.Etapa)
            .Include(p => p.Zahtjev)
            .Include(p => p.VoditeljEmailNavigation);

            planovi = planovi.ApplySort(sort, ascending);

            var planoviSorted = planovi
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new PlanProjektaPageView
            {
                Planovi = planoviSorted,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> MasterDetailEdit(int PlanProjektaId)
        {
            var plan = await _context.PlanProjekta
                                        .Include(p => p.Projekt)
                                        .Include(p => p.Etapa)
                                        .Include(p => p.Zahtjev)
                                        .Include(p => p.VoditeljEmailNavigation)
                                        .Where(p => p.PlanProjektaId == PlanProjektaId)
                                        .Select(p => new PlanProjektaViewModel
                                        {
                                            PlanProjekta = p,
                                        })
                                        .FirstOrDefaultAsync();
            if (plan == null)
            {
                return NotFound($"Plan projekta {PlanProjektaId} ne postoji");
            }
            else
            {
                //učitavanje etapa
                var etape = await _context.Etapa
                                      .Include(e => e.Aktivnost)
                                      .Where(e => e.PlanProjektaId == plan.PlanProjekta.PlanProjektaId)
                                      .OrderBy(e => e.EtapaId)
                                      .ToListAsync();

                plan.Etape = etape;
                return View(plan);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MasterDetailEdit(PlanProjektaViewModel viewModel, int PlanProjektaId)
        {
            if (ModelState.IsValid)
            {
                var plan = await _context.PlanProjekta
                                        .Include(p => p.Projekt)
                                        .Include(p => p.Etapa)
                                        .Include(p => p.Zahtjev)
                                        .Include(p => p.VoditeljEmailNavigation)
                                        .Where(p => p.PlanProjektaId == PlanProjektaId)
                                        .FirstOrDefaultAsync();

                plan.PlaniraniPočetak = viewModel.PlanProjekta.PlaniraniPočetak;
                plan.PlaniraniKraj = viewModel.PlanProjekta.PlaniraniKraj;
                plan.StvarniPočetak = viewModel.PlanProjekta.StvarniPočetak;
                plan.StvarniKraj = viewModel.PlanProjekta.StvarniKraj;
                plan.ProjektId = viewModel.PlanProjekta.ProjektId;
                plan.VoditeljEmail = viewModel.PlanProjekta.VoditeljEmail;

                try
                {
                    _context.SaveChanges();

                    TempData[Constants.ErrorOccurred] = false;
                    TempData["SuccessMessage"] = $"Izmjene nad plan projekta {PlanProjektaId} spremljene.";
                    return RedirectToAction(nameof(MasterDetailEdit),
                        new { PlanProjektaId = PlanProjektaId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.ToString());
                    TempData[Constants.ErrorOccurred] = true;
                    TempData["errorMessage"] = $"Nije moguće spremiti izmjene nad Planom projekta {viewModel.PlanProjekta.PlanProjektaId}.";
                    return View(viewModel);
                }
            }
            else
            {
                return View(viewModel);
            }
        }

        [HttpGet]
        public IActionResult MasterDetailCreate()
        {
            var viewModel = new PlanProjektaViewModel
            {
                PlanProjekta = new PlanProjekta(),
                Etape = new List<Etapa>(),
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MasterDetailCreate(PlanProjektaViewModel viewModel)
        {
            var plan = new PlanProjekta
            {
                PlaniraniPočetak = viewModel.PlanProjekta.PlaniraniPočetak,
                PlaniraniKraj = viewModel.PlanProjekta.PlaniraniKraj,
                StvarniPočetak = viewModel.PlanProjekta.StvarniPočetak,
                StvarniKraj = viewModel.PlanProjekta.StvarniKraj,
                ProjektId = viewModel.PlanProjekta.ProjektId,
                VoditeljEmail = viewModel.PlanProjekta.VoditeljEmail,
            };

            var result = validator.Validate(plan);
            if (result.IsValid)
            {
                _context.PlanProjekta.Add(plan);

                foreach (var etapa in viewModel.Etape)
                {
                    var newAktivnost = _context.Aktivnost.Find(etapa.AktivnostId);

                    var newEtapa = new Etapa();
                    newEtapa.Ime = etapa.Ime;
                    newEtapa.Opis = etapa.Opis;
                    newEtapa.PlanProjektaId = plan.PlanProjektaId;
                    newEtapa.AktivnostId = etapa.AktivnostId;
                    newEtapa.PlanProjekta = plan;
                    newEtapa.Aktivnost = newAktivnost;
                    _context.Etapa.Add(newEtapa);
                }

                _context.SaveChanges();

                TempData[Constants.ErrorOccurred] = false;
                TempData["SuccessMessage"] = $"Plan projekta {plan.PlanProjektaId} dodan.";
                return RedirectToAction(nameof(Index2));
            }
            TempData["errorMessage"] = $"Plan projekta nije moguće dodati.";
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete2(int PlanProjektaId, int page = 1, int sort = 1, bool ascending = true)
        {
            var planToBeDeleted = _context.PlanProjekta.Find(PlanProjektaId);
            if (planToBeDeleted != null)
            {
                try
                {
                    _context.Remove(planToBeDeleted);
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = $"Plan projekta {planToBeDeleted.PlanProjektaId} uspješno obrisan.";
                }
                catch (Exception ex)
                {
                    TempData["errorMessage"] = $"Plan projekta {planToBeDeleted.PlanProjektaId} se ne može obrisati.";
                }
            }
            else
            {
                TempData["errorMessage"] = $"Plan projekta {planToBeDeleted.PlanProjektaId} se ne može obrisati.";
            }

            return RedirectToAction(nameof(Index2), new { page = page, sort = sort, ascending = ascending });
        }

        [HttpGet]
        public IActionResult UvozIzvoz()
        {
            return View();
        }
    }
}
