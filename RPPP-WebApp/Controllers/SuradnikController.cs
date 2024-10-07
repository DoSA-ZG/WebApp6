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
using RPPP_WebApp.ModelsPartial;
using NLog.Filters;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace RPPP_WebApp.Controllers
{
    public class SuradnikController : Controller
    {
        private readonly RPPP06Context _context;
        private readonly AppSettings appSettings;
        private readonly ILogger<SuradnikController> logger;
        SuradnikValidator suradnikValidator = new SuradnikValidator();


        public SuradnikController(RPPP06Context context, IOptionsSnapshot<AppSettings> options, ILogger<SuradnikController> logger)
        {
            _context = context;
            appSettings = options.Value;
            this.logger = logger;
        }
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            //IEnumerable<Suradnik> objSuradnikList = _context.Suradnik.Include(s => s.Uloga).Include(s => s.Posao);
            var objSuradnikList = _context.Suradnik.AsNoTracking();
            int count = objSuradnikList.Count();
            if (count == 0)
            {
                logger.LogInformation("Ne postoji nijedna država");
                TempData[Constants.Message] = "Ne postoji niti jedna država.";
                TempData[Constants.ErrorOccurred] = false;
                return RedirectToAction(nameof(Create));
            }

            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending,
                ItemsPerPage = pagesize,
                TotalItems = count
            };
            if (page < 1 || page > pagingInfo.TotalPages)
            {
                return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
            }
            objSuradnikList = objSuradnikList.Include(s => s.Uloga.OrderBy(u => u.Ime)).ApplySort(sort, ascending);

            var objSuradnik = objSuradnikList

                        .Skip((page - 1) * pagesize)
                        .Take(pagesize)
                        .ToList();

            var model = new SuradnikPageView
            {
                Suradnik = objSuradnik,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        public IActionResult Master(int page = 1, int sort = 1, bool ascending = true)
        {
            //IEnumerable<Suradnik> objSuradnikList = _context.Suradnik.Include(s => s.Uloga).Include(s => s.Posao);
            int pagesize = appSettings.PageSize;
            var objSuradnikList = _context.Suradnik.AsNoTracking();
            int count = objSuradnikList.Count();

            if (count == 0)
            {
                logger.LogInformation("Ne postoji nijedna država");
                TempData[Constants.Message] = "Ne postoji niti jedna država.";
                TempData[Constants.ErrorOccurred] = false;
                return RedirectToAction(nameof(Create));
            }

            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending,
                ItemsPerPage = pagesize,
                TotalItems = count
            };
            if (page < 1 || page > pagingInfo.TotalPages)
            {
                return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
            }
            objSuradnikList = objSuradnikList.Include(s => s.Posao.OrderBy(u => u.Opis)).Include(s => s.Uloga.OrderBy(u => u.Ime)).ApplySort(sort, ascending);

            var objSuradnik = objSuradnikList

                      .Skip((page - 1) * pagesize)
                      .Take(pagesize)
                      .ToList();

            var model = new SuradnikPageView
            {
                Suradnik = objSuradnik,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        //GET
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            Suradnik suradnik1 = new Suradnik();
            List<Uloga> uloge = _context.Uloga.ToList();
            var viewModel = new SuradnikViewModel
            {
                suradnik = suradnik1,
                SelectedUloge = null
            };
            await PrepareDropDownLists();
            await PrepareDropDownListsProjekt();
            ViewBag.UlogeList = uloge;
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SuradnikViewModel suradnikViewModel)
        {
            Suradnik suradnik = suradnikViewModel.suradnik;

            var rezultatValidacije = suradnikValidator.Validate(suradnik);

            if (!rezultatValidacije.IsValid)
            {
                foreach (var error in rezultatValidacije.Errors)
                {
                    Console.WriteLine($"Property: {error.PropertyName}, Error: {error.ErrorMessage}");
                }

                // Ovdje možete poduzeti dodatne akcije kad je validacija neuspješna
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Suradnik.Add(suradnikViewModel.suradnik);
                    _context.SaveChanges();
                    var suradnikId = suradnikViewModel.suradnik.Email;
                    if (!string.IsNullOrEmpty(suradnikId) && suradnikViewModel.SelectedUloge != null)
                    {
                        // Get the Uloga objects from the context
                        var uloge = _context.Uloga.Where(u => suradnikViewModel.SelectedUloge.Any(selectedUloga => selectedUloga == u.Ime)).ToList();

                        // Associate Uloge with the Suradnik
                        suradnikViewModel.suradnik.Uloga = uloge;

                        // Save changes to persist the association
                        _context.SaveChanges();

                    }
                    if (!string.IsNullOrEmpty(suradnikId) && suradnikViewModel.SelectedProjekt != null)
                    {
                        // Get the Uloga objects from the context
                        var projekti = _context.Projekt.Where(u => suradnikViewModel.SelectedProjekt.Any(selectedProjekt => selectedProjekt == u.Ime)).ToList();

                        // Associate Uloge with the Suradnik
                        suradnikViewModel.suradnik.Projekt = projekti;

                        // Save changes to persist the association
                        _context.SaveChanges();

                    }
                    TempData[Constants.ErrorOccurred] = false;
                    TempData["SuccessMessage"] = $"Suradnik {suradnikViewModel.suradnik.Email} dodan.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    
                    ModelState.AddModelError("", "An error occurred while saving the Suradnik.");
                    TempData["errorMessage"] = $"Suradnik {suradnikViewModel.suradnik.Email} nije moguće dodati.";
                   
                }
            }
            await PrepareDropDownLists();
            await PrepareDropDownListsProjekt();
            List<Uloga> uloge1 = _context.Uloga.ToList();
            ViewBag.UlogeList = uloge1;
            return View(suradnikViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string Email, int page = 1, int sort = 1, bool ascending = true)
        {
            var suradnik = _context.Suradnik.Include(s => s.Posao).FirstOrDefault(s => s.Email == Email);

            if (suradnik != null)
            {
                try
                {
                    suradnik = _context.Suradnik.Find(Email);

                    if(suradnik.Posao != null)
                    {
                        foreach (var posao in suradnik.Posao)
                        {
                            _context.Remove(posao);
                        }
                    }

                    _context.Remove(suradnik);
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = $"Suradnik {suradnik.Email} uspješno obrisan.";
                }
                catch (Exception ex)
                {
                    TempData["errorMessage"] = $"Suradnik {suradnik.Email} se ne može obrisati jer je nositelj zadatka!.";
                }
            }
            else
            {
                //Greške TempData i logger
            }
            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string Email, int page = 1, int sort = 1, bool ascending = true)
        {
            var suradnik = _context.Suradnik.AsNoTracking().Include(s => s.Uloga).Include(s => s.Projekt).Where(d => d.Email == Email).SingleOrDefault();
            if (suradnik == null)
            {
                
                return NotFound("Ne postoji suradnik s oznakom: " + Email);
            }
            else
            {
                
                List<Uloga> uloge = _context.Uloga.ToList();
                var viewModel = new SuradnikViewModel
                {
                    suradnik = suradnik,
                    SelectedUloge = suradnik.Uloga.Select(u => u.Ime).ToList()
                };
                await PrepareDropDownLists();
                await PrepareDropDownListsProjekt();
                ViewBag.UlogeList = uloge;
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                return View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SuradnikViewModel suradnikView, int page = 1, int sort = 1, bool ascending = true)
        {
            Suradnik suradnik = suradnikView.suradnik;

            var rezultatValidacije = suradnikValidator.Validate(suradnik);

            if (!rezultatValidacije.IsValid)
            {
                foreach (var error in rezultatValidacije.Errors)
                {
                    Console.WriteLine($"Property: {error.PropertyName}, Error: {error.ErrorMessage}");
                }

                // Ovdje možete poduzeti dodatne akcije kad je validacija neuspješna
            }
            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;
            var existingSuradnik = await _context.Suradnik
                   .Include(s => s.Uloga).Include(s => s.Projekt)
                   .FirstOrDefaultAsync(s => s.Email == suradnikView.suradnik.Email);
            if (ModelState.IsValid)
            {
                try
                {

                    // Retrieve the existing Suradnik from the database

                    if (existingSuradnik == null)
                    {
                        return NotFound("Suradnik nije pronađen");
                    }

                    // Update properties of the existing Suradnik with data from the view model
                    //existingSuradnik.Email = suradnikView.suradnik.Email;
                    existingSuradnik.Ime = suradnikView.suradnik.Ime;
                    existingSuradnik.Prezime = suradnikView.suradnik.Prezime;
                    existingSuradnik.MjestoStanovanja = suradnikView.suradnik.MjestoStanovanja;
                    existingSuradnik.BrojTelefona = suradnikView.suradnik.BrojTelefona;
                    existingSuradnik.URL = suradnikView.suradnik.URL;
                    existingSuradnik.NadređeniEmail = suradnikView.suradnik.NadređeniEmail;

                    // Update the relationship with Uloga based on SelectedUloge
                    if (suradnikView.SelectedUloge != null)
                    {

                        var selectedUloge = await _context.Uloga
                            .Where(u => suradnikView.SelectedUloge.Any(selectedUloga => selectedUloga == u.Ime))
                            .ToListAsync();


                        existingSuradnik.Uloga = selectedUloge;
                    }
                    else
                    {
                        // If no Uloga is selected, you may want to clear the existing associations
                        existingSuradnik.Uloga.Clear();
                    }
                    if (suradnikView.SelectedProjekt != null)
                    {
                        
                        // Get the Uloga objects from the context
                        var projekti = await _context.Projekt.Where(u => suradnikView.SelectedProjekt.Any(selectedProjekt => selectedProjekt == u.Ime)).ToListAsync();

                        // Associate Uloge with the Suradnik
                       existingSuradnik.Projekt = projekti;

                        // Save changes to persist the asso
                    }
                    else
                    {
                        // If no Uloga is selected, you may want to clear the existing associations
                        existingSuradnik.Projekt.Clear();
                    }


                    // Save changes to persist the updates
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Suradnik {existingSuradnik.Email} uspješno promijenjen.";
                    return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
                }
                catch (Exception ex)
                {
                    // Handle the exception, log it, or display an error message
                    TempData["errorMessage"] = $"Suradnik {existingSuradnik.Email} neuspješno promijenjen.";
                    // You may want to return to the same view to display the error.
                    await PrepareDropDownLists();
                    await PrepareDropDownListsProjekt();
                    List<Uloga> uloge = _context.Uloga.ToList();
                    ViewBag.UlogeList = uloge;
                    return View(suradnikView);
                }
            }
            await PrepareDropDownLists();
            TempData["errorMessage"] = $"Suradnik {existingSuradnik.Email} neuspješno promijenjen.";
            List<Uloga> uloge2 = _context.Uloga.ToList();
            ViewBag.UlogeList = uloge2;
            return View(suradnikView);
        }

        public IActionResult Detail(string Email)
        {
            var suradnik = _context.Suradnik
                 .AsNoTracking()
                 .Include(s => s.Uloga)
                 .Include(s => s.Posao)
                     .ThenInclude(p => p.VrstaPosla)
                 .Include(s => s.Posao)
                     .ThenInclude(p => p.Zadatak)
                         .ThenInclude(p => p.Zahtjev)
                             .ThenInclude(p => p.PlanProjekta)
                                 .ThenInclude(p => p.Projekt) // Include the Zadatak navigation property within Posao
                 .Where(d => d.Email == Email)
                 .SingleOrDefault();


            if (suradnik == null)
            {
                return NotFound("Suradnik not found");
            }

            return View(suradnik);
        }
        //--------------------------------------------------------------- 2. provjera

        [HttpGet]
        public async Task<IActionResult> Create2()
        {

            var viewModel = new SuradnikDetailView
            {
            
            };
            List<Uloga> uloge1 = _context.Uloga.ToList();
            ViewBag.UlogeList = uloge1;
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create2(SuradnikDetailView suradnikDetailView)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    Suradnik suradnik = new Suradnik();
                    suradnik.Email = suradnikDetailView.Email;
                    suradnik.Ime = suradnikDetailView.Ime;
                    suradnik.Prezime = suradnikDetailView.Prezime;
                    suradnik.MjestoStanovanja = suradnikDetailView.MjestoStanovanja;
                    suradnik.BrojTelefona = suradnikDetailView.BrojTelefona;
                    suradnik.URL = suradnikDetailView.URL;
                    suradnik.NadređeniEmail = suradnikDetailView.NadredeniEmail;
                    _context.Suradnik.Add(suradnik);
                    _context.SaveChanges();

                    List<String> SelectedUloge = new List<string>();
                    for (int i = 0; i < 5; i++)
                    {
                        var uloga = suradnikDetailView.Uloga.ElementAt(i);
                        if (uloga != null)
                        {
                            SelectedUloge.Add(uloga.Ime);
                        }
                    }

                    if (SelectedUloge != null)
                    {

                        var selectedUloge = await _context.Uloga
                            .Where(u => SelectedUloge.Any(selectedUloga => selectedUloga == u.Ime))
                            .ToListAsync();


                        suradnik.Uloga = selectedUloge;
                    }

                    foreach (var posao in suradnikDetailView.Posao)
                    {
                        //ažuriraj postojeće i dodaj nove
                        Posao noviPosao; // potpuno nova ili dohvaćena ona koju treba izmijeniti
                        if (posao.PosaoId > 0 && posao.PosaoId < 1000)
                        {
                            noviPosao = suradnik.Posao.First(s => s.PosaoId == posao.PosaoId);
                        }
                        else
                        {
                            noviPosao = new Posao();
                            suradnik.Posao.Add(noviPosao);
                        }
                        noviPosao.Opis = posao.Opis;
                        noviPosao.VrijemePočetkaRada = posao.VrijemePočetkaRada;
                        noviPosao.VrijemeKrajaRada = posao.VrijemeKrajaRada;
                        noviPosao.ZadatakId = posao.ZadatakId;
                        noviPosao.SuradnikEmail = suradnik.Email;



                        var vrstaPosla = await _context.VrstaPosla
                            .Where(u => u.VrstaPoslaId == posao.VrstaPoslaId)
                            .FirstOrDefaultAsync();

                        var nadređeniSuradnik = await _context.Suradnik
                            .Where(u => u.Email == suradnik.NadređeniEmail)
                            .FirstOrDefaultAsync();

                        var zadatak = await _context.Zadatak.Include(z => z.Zahtjev).ThenInclude(z => z.PlanProjekta).ThenInclude(z => z.Projekt)
                            .Where(u => u.ZadatakId == posao.ZadatakId)
                            .FirstOrDefaultAsync();

                        noviPosao.VrstaPoslaId = vrstaPosla.VrstaPoslaId;
                        posao.VrstaPosla = vrstaPosla;
                        posao.Zadatak = zadatak;
                        suradnikDetailView.nadredeniSuradnik = nadređeniSuradnik;

                    }

                    await _context.SaveChangesAsync();
                    TempData[Constants.ErrorOccurred] = false;
                    TempData["SuccessMessage"] = $"Suradnik {suradnikDetailView.Email} dodan.";
                    return RedirectToAction(nameof(Master2));
                }
                catch (Exception ex)
                {

                    ModelState.AddModelError("", "An error occurred while saving the Suradnik.");
                    TempData["errorMessage"] = $"Suradnik {suradnikDetailView.Email} nije moguće dodati.";

                }
            }
            List<Uloga> uloge1 = _context.Uloga.ToList();
            ViewBag.UlogeList = uloge1;
            return View(suradnikDetailView);
        }


        public IActionResult Master2(int page = 1, int sort = 1, bool ascending = true)
        {
            //IEnumerable<Suradnik> objSuradnikList = _context.Suradnik.Include(s => s.Uloga).Include(s => s.Posao);
            int pagesize = appSettings.PageSize;
            var objSuradnikList = _context.Suradnik.AsNoTracking();
            int count = objSuradnikList.Count();

            if (count == 0)
            {
                logger.LogInformation("Ne postoji nijedna država");
                TempData[Constants.Message] = "Ne postoji niti jedna država.";
                TempData[Constants.ErrorOccurred] = false;
                return RedirectToAction(nameof(Create));
            }

            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending,
                ItemsPerPage = pagesize,
                TotalItems = count
            };
            if (page < 1 || page > pagingInfo.TotalPages)
            {
                return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
            }
            objSuradnikList = objSuradnikList.Include(s => s.Posao.OrderBy(u => u.Opis)).Include(s => s.Uloga.OrderBy(u => u.Ime)).ApplySort(sort, ascending);

            var objSuradnik = objSuradnikList

                      .Skip((page - 1) * pagesize)
                      .Take(pagesize)
                      .ToList();

            List<ViewSuradnikInfo> viewSuradnik = new List<ViewSuradnikInfo>();

            for (int i = 0; i < objSuradnik.Count; i++)
            {
                var position = (page - 1) * pagesize + i;
                var suradnik = objSuradnik[i];

                ViewSuradnikInfo viewSuradnikInfo = new ViewSuradnikInfo
                {
                    suradnik = suradnik,
                    Position = position
                };
                viewSuradnik.Add(viewSuradnikInfo);
            }


            var model = new SuradnikViewModel2
            {
                Suradnik = viewSuradnik,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Show(string email, int? position, int page = 1, int sort = 1, bool ascending = true, string viewName = nameof(Show))
        {
            //var suradnik = _context.Suradnik
            //     .AsNoTracking()
            //     .Include(s => s.Uloga)
            //     .Include(s => s.Posao)
            //         .ThenInclude(p => p.VrstaPosla)
            //     .Include(s => s.Posao)
            //         .ThenInclude(p => p.Zadatak)
            //             .ThenInclude(p => p.Zahtjev)
            //                 .ThenInclude(p => p.PlanProjekta)
            //                     .ThenInclude(p => p.Projekt) 
            //     .Where(d => d.Email == email)
            //     .SingleOrDefault();

            var suradnik = await _context.Suradnik.Where(d => d.Email == email).Include(s => s.Uloga)
                 .Include(s => s.Posao)
                     .ThenInclude(p => p.VrstaPosla)
                 .Include(s => s.Posao)
                     .ThenInclude(p => p.Zadatak)
                         .ThenInclude(p => p.Zahtjev)
                             .ThenInclude(p => p.PlanProjekta)
                                 .ThenInclude(p => p.Projekt).Select(d => new SuradnikDetailView
            {
                Email = d.Email,
                Ime = d.Ime,
                Prezime = d.Prezime,
                MjestoStanovanja = d.MjestoStanovanja,
                BrojTelefona = d.BrojTelefona,
                URL = d.URL,
                NadredeniEmail = d.NadređeniEmail,
                Posao = d.Posao,
                Uloga = d.Uloga
            }).FirstOrDefaultAsync();

            var suradnik2 = await _context.Suradnik.Where(d => d.Email == email).FirstOrDefaultAsync();
            suradnik.nadredeniSuradnik = await _context.Suradnik.Where(d => d.Email == suradnik.NadredeniEmail).FirstOrDefaultAsync();
            if (suradnik == null)
            {
                return NotFound("Suradnik not found");
            }

            if (position.HasValue)
            {
                page = 1 + position.Value / appSettings.PageSize;
                await SetPreviousAndNext(position.Value, sort, ascending);
            }
            List<Uloga> uloge = _context.Uloga.ToList();
            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;
            ViewBag.Position = position;
            ViewBag.UlogeList = uloge;
            await PrepareDropDownListsVrstaPosla();

            return View(viewName, suradnik);
        }



        private async Task SetPreviousAndNext(int position, int sort, bool ascending)
        {
            var query = _context.Suradnik.AsQueryable();


            query = query.ApplySort(sort, ascending);
            if (position > 0)
            {
                ViewBag.Previous = await query.Skip(position - 1).Select(d => d.Email).FirstAsync();
            }
            if (position < await query.CountAsync() - 1)
            {
                ViewBag.Next = await query.Skip(position + 1).Select(d => d.Email).FirstAsync();
            }
        }

        [HttpGet]
        public Task<IActionResult> Edit2(string email, int? position, int page = 1, int sort = 1, bool ascending = true)
        {
            return Show(email, position, page, sort, ascending, viewName: nameof(Edit2));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit2(SuradnikDetailView model, int? position,int page = 1, int sort = 1, bool ascending = true)
        {
            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;
            ViewBag.Position = position;
            if (ModelState.IsValid)
            {
                var suradnik = await _context.Suradnik
                                        .Include(d => d.Posao)
                                        .Include(s => s.Uloga)
                                        .Where(d => d.Email == model.Email)
                                        .FirstOrDefaultAsync();
                if (suradnik == null)
                {
                    return NotFound("Ne postoji suradnik s email-om: " + model.Email);
                }

                if (position.HasValue)
                {
                    page = 1 + position.Value / appSettings.PageSize;
                    await SetPreviousAndNext(position.Value,sort, ascending);
                }

                suradnik.Ime = model.Ime;
                suradnik.Prezime = model.Prezime;
                suradnik.MjestoStanovanja = model.MjestoStanovanja;
                suradnik.BrojTelefona = model.BrojTelefona;
                suradnik.URL = model.URL;
                suradnik.NadređeniEmail = model.NadredeniEmail;


                List<String> SelectedUloge = new List<string>();
                for(int i = 0; i < 5; i++) {
                    var uloga = model.Uloga.ElementAt(i);
                    if (uloga != null)
                    {
                        SelectedUloge.Add(uloga.Ime);
                    }
                }

                if (SelectedUloge != null)
                {

                    var selectedUloge = await _context.Uloga
                        .Where(u => SelectedUloge.Any(selectedUloga => selectedUloga == u.Ime))
                        .ToListAsync();


                    suradnik.Uloga = selectedUloge;
                }
                else
                {
                    // If no Uloga is selected, you may want to clear the existing associations
                    suradnik.Uloga.Clear();
                }

                List<int> idPosao = model.Posao
                                  .Where(s => s.PosaoId > 0)
                                  .Select(s => s.PosaoId)
                                  .ToList();

                _context.RemoveRange(suradnik.Posao.Where(s => !idPosao.Contains(s.PosaoId)));
                var prolaz = false;
                foreach (var posao in model.Posao)
                {
                    //ažuriraj postojeće i dodaj nove
                    Posao noviPosao; // potpuno nova ili dohvaćena ona koju treba izmijeniti
                    if (posao.PosaoId > 0 && posao.PosaoId < 1000)
                    {
                        noviPosao = suradnik.Posao.First(s => s.PosaoId == posao.PosaoId);
                    }
                    else
                    {
                        noviPosao = new Posao();
                        suradnik.Posao.Add(noviPosao);
                    }
                    noviPosao.Opis = posao.Opis;
                    noviPosao.VrijemePočetkaRada = posao.VrijemePočetkaRada;
                    noviPosao.VrijemeKrajaRada = posao.VrijemeKrajaRada;
                    noviPosao.ZadatakId = posao.ZadatakId;
                    if(noviPosao.VrijemeKrajaRada < noviPosao.VrijemePočetkaRada)
                    {
                        prolaz = true;
                    }
                    var vrstaPosla = await _context.VrstaPosla
                        .Where(u => u.VrstaPoslaId == posao.VrstaPoslaId)
                        .FirstOrDefaultAsync();

                    var zadatak = await _context.Zadatak.Include(z => z.Zahtjev).ThenInclude(z => z.PlanProjekta).ThenInclude(z => z.Projekt)
                        .Where(u => u.ZadatakId == posao.ZadatakId)
                        .FirstOrDefaultAsync();


                    noviPosao.VrstaPoslaId = vrstaPosla.VrstaPoslaId;
                    posao.VrstaPosla = vrstaPosla;
                    posao.Zadatak = zadatak;

                }
                try
                {
                    if (prolaz == true)
                    {
  
                        List<string> ulogeime = new List<string>();
                        for(int i = 0; i < _context.Uloga.Count(); i++)
                        {
                            if(model.Uloga.ElementAt(i) != null)
                            {
                                ulogeime.Add(model.Uloga.ElementAt(i).Ime);
                            }
                        }
                     
                        List<Uloga> uloga = _context.Uloga.Where(u => ulogeime.Contains(u.Ime)).ToList();

                        model.Uloga = uloga;

                        model.nadredeniSuradnik = _context.Suradnik.Where(u => u.Email == model.NadredeniEmail).FirstOrDefault();

                        TempData["errorMessage"] = $"Vrijeme početka posla mora biti prije vremena kraja posla!";
                        List<Uloga> uloge = _context.Uloga.ToList();
                        ViewBag.Page = page;
                        ViewBag.Sort = sort;
                        ViewBag.Ascending = ascending;
                        ViewBag.Position = position;
                        ViewBag.UlogeList = uloge;
                        return View(model);
                    }
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Suradnik {suradnik.Email} uspješno promijenjen.";
                    return RedirectToAction(nameof(Edit2), new
                    {
                        email = suradnik.Email,
                        position,
                        page,
                        sort,
                        ascending
                    });

                }
                catch (Exception exc)
                {
                    List<Uloga> uloge = _context.Uloga.ToList();
                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                    ViewBag.Position = position;
                    ViewBag.UlogeList = uloge;
                    await PrepareDropDownListsVrstaPosla();
                    TempData["errorMessage"] = $"Suradnik {suradnik.Email} neuspješno promijenjen.";
                    return RedirectToAction(nameof(Edit2), new
                    {
                        email = suradnik.Email,
                        position,
                        page,
                        sort,
                        ascending
                    });
                }
            }
            else
            {
                List<Uloga> uloge = _context.Uloga.ToList();
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                ViewBag.Position = position;
                ViewBag.UlogeList = uloge;
                await PrepareDropDownListsVrstaPosla();
                TempData["errorMessage"] = $"Sva polja poslova moraju biti popunjena!.";
                return RedirectToAction(nameof(Edit2), new
                {
                    email = model.Email,
                    position,
                    page,
                    sort,
                    ascending
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete2(string Email, int page = 1, int sort = 1, bool ascending = true)
        {
            var suradnik = _context.Suradnik.Include(s => s.Posao).FirstOrDefault(s => s.Email == Email);

            if (suradnik != null)
            {
                try
                {
                    suradnik = _context.Suradnik.Find(Email);

                    if (suradnik.Posao != null)
                    {
                        foreach (var posao in suradnik.Posao)
                        {
                            _context.Remove(posao);
                        }
                    }

                    _context.Remove(suradnik);
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = $"Suradnik {suradnik.Email} uspješno obrisan.";
                }
                catch (Exception ex)
                {
                    TempData["errorMessage"] = $"Suradnik {suradnik.Email} se ne može obrisati!.";
                }
            }
            else
            {
                //Greške TempData i logger
            }
            return RedirectToAction(nameof(Master2), new { page = page, sort = sort, ascending = ascending });
        }

        [HttpGet]
        public IActionResult UvozIzvoz()
        {
            return View();
        }



        private async Task PrepareDropDownLists()
        {

            var suradnici = await _context.Suradnik
                          .OrderBy(d => d.Ime)
                          .Select(d => new SelectListItem
                          {
                              Value = d.Email,  // Set the appropriate property as the value
                              Text = $"{d.Ime} {d.Prezime}"  // Set the appropriate properties as the text
                          })
                          .ToListAsync();

            ViewBag.Suradnici = new SelectList(suradnici, nameof(SelectListItem.Value), nameof(SelectListItem.Text));

        }
        private async Task PrepareDropDownListsProjekt()
        {

            var suradnici = await _context.Projekt
                          .OrderBy(d => d.Ime)
                          .Select(d => new SelectListItem
                          {
                              Value = d.Ime,  // Set the appropriate property as the value
                              Text = $"{d.Ime}"  // Set the appropriate properties as the text
                          })
                          .ToListAsync();

            ViewBag.Projekt = new SelectList(suradnici, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
        }

        private async Task PrepareDropDownListsVrstaPosla()
        {

            var vrstaPosla = await _context.VrstaPosla
                          .OrderBy(d => d.Ime)
                          .Select(d => new SelectListItem
                          {
                              Value = d.Ime,  // Set the appropriate property as the value
                              Text = $"{d.Ime}"  // Set the appropriate properties as the text
                          })
                          .ToListAsync();

            ViewBag.vrstaPosla = new SelectList(vrstaPosla, nameof(SelectListItem.Value), nameof(SelectListItem.Text));

        }

    }


}
