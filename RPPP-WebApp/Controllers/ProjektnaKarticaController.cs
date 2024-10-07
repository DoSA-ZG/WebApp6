using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Data;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Diagnostics;
using RPPP_WebApp.Exstensions.Selectors;
using RPPP_WebApp.ModelsValidation;
using System.Drawing.Printing;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.Controllers
{
    /// <summary>
    /// Razred za upravljanje projektnim karticama i prikazivanje View-ova
    /// Nasljeđuje Controller razred iz ASP.NET Core MVC biblioteke
    /// </summary>
    /// <param name="context">Kontekst za pristup bazi podataka</param>
    /// <param name="logger">Logger za ispis logova</param>
    public class ProjektnaKarticaController(RPPP06Context context, ILogger<ProjektnaKarticaController> logger) : Controller
    {
        private readonly RPPP06Context _context = context;
        private readonly ILogger<ProjektnaKarticaController> logger = logger;

        /// <summary>
        /// Validator za provjeru unešenih podataka za projektne kartice
        /// </summary>
        private ProjektnaKarticaValidator validator = new ProjektnaKarticaValidator();

        
        /// <summary>
        /// Endpoint koji se poziva prilikom pristupa stranici Index projektne kartica, dohvaća sve projektne kartice iz baze podataka i prikazuje ih u View-u
        /// </summary>
        /// <returns>View sa listom projektnih kartica</returns>
        public IActionResult Index()
        {
            IEnumerable<ProjektnaKartica> projektneKartice = _context.ProjektnaKartica
            .Include(p => p.Projekt)
            .ToList();

            return View(projektneKartice);
        }

        /// <summary>
        /// Endpoint koji se poziva prilikom pristupa Master-u projektne kartice, dohvaća sve projektne kartice iz baze podataka i prikazuje ih u View-u
        /// </summary>
        /// <returns>View sa listom projektnih kartica</returns>
        public IActionResult Master()
        {
            IEnumerable<ProjektnaKartica> projektneKartice = _context.ProjektnaKartica
            .Include(p => p.Projekt)
            .ToList();

            return View(projektneKartice);
        }

        /// <summary>
        /// Endpoint koji se poziva prilikom pristupa detaljima projektne kartice, dohvaća projektnu karticu iz baze podataka i prikazuje ju u View-u
        /// </summary>
        /// <param name="id">Id projektne kartice koja se želi prikazati</param>
        /// <returns>View sa detaljima o projektnoj kartici</returns>
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kartica = _context.ProjektnaKartica
                .Include(p => p.Projekt)
                .FirstOrDefault(m => m.ProjektnaKarticaId == id);

            if (kartica == null)
            {
                return NotFound();
            }

            return View(kartica);
        }



        /// <summary>
        /// Metoda koja se poziva prilikom pristupa stranici Create, te prikazuje View za stvaranje nove projektne kartice
        /// </summary>
        /// <returns>View za stvaranje nove projektne kartice</returns>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareProjektnaKarticaProps();
            ProjektnaKartica kartica = new ProjektnaKartica();
            var viewModel = new ProjektnaKarticaViewModel
            {
                ProjektnaKartica = kartica
            };
            return View(viewModel);
        }


        /// <summary>
        /// Metoda koja se poziva prilikom pristupa stranici Edit, te prikazuje View za uređivanje projektne kartice
        /// </summary>
        /// <param name="ProjektnaKarticaId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int ProjektnaKarticaId)
        {
            await PrepareProjektnaKarticaProps();
            ProjektnaKartica kartica = new ProjektnaKartica();
            kartica = _context.ProjektnaKartica.Find(ProjektnaKarticaId);
            var viewModel = new ProjektnaKarticaViewModel
            {
                ProjektnaKartica = kartica
            };
            return View(viewModel);
        }


        /// <summary>
        /// Endpoint koji se poziva prilikom zahtjeva za stvaranje nove projektne kartice, provjerava ispravnost podataka i sprema novu projektnu karticu u bazu podataka
        /// </summary>
        /// <param name="viewModel">ViewModel sa podacima o novoj projekt kartici</param>
        /// <returns>View sa detaljima o novoj projekt kartici</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProjektnaKarticaViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    viewModel.ProjektnaKartica.ProjektId = viewModel.Projekt.ProjektId;

                    _context.ProjektnaKartica.Add(viewModel.ProjektnaKartica);
                    _context.SaveChanges();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", e.GetBaseException().Message);
                }
            }
            return View(viewModel);
        }


        /// <summary>
        /// Endpoint koji se poziva prilikom zahtjeva za brisanje projektne kartice, provjerava postojanje projektne kartice u bazi podataka i briše ju
        /// </summary>
        /// <param name="ProjektnaKarticaId">Id projektne kartice koja se želi obrisati</param>
        /// <returns>Reditect na Master projektne kartice</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int ProjektnaKarticaId)
        {
            var karticaToBeDeleted = _context.ProjektnaKartica.Find(ProjektnaKarticaId);
            if (karticaToBeDeleted != null)
            {
                try
                {
                    _context.Remove(karticaToBeDeleted);
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: {0}", ex.GetBaseException().Message);
                }
            }

            return RedirectToAction(nameof(Master2));
        }


        /// <summary>
        /// Metoda koja zove metode za pripremu podataka za dropdown liste i sl.
        /// </summary>
        /// <returns></returns>
        private async Task PrepareProjektnaKarticaProps()
        {
            await PrepareProjektDropDownList();
        }


        /// <summary>
        /// Metoda koja priprema podatke za dropdown listu projekatnih kartica
        /// </summary>
        /// <returns></returns>
        private async Task PrepareProjektDropDownList()
        {
            var Projekti = await _context.Projekt
                        .OrderBy(t => t.ProjektId)
                        .Select(d => new SelectListItem
                        {
                            Value = d.ProjektId.ToString(),
                            Text = d.Ime

                        }).ToListAsync();

            ViewBag.Projekti = new SelectList(Projekti, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
        }

        // DRUGA PREDAJA -----------------------------------


        /// <summary>
        /// Endpoint koji se poziva prilikom kreiranja nove projektne kartice sa master-detail view-a, prikazuje View za stvaranje nove projektne kartice
        /// </summary>
        /// <returns>View za stvaranje nove projektne kartice</returns>
        [HttpGet]
        public IActionResult Create2()
        {
            var viewModel = new ProjektnaKarticaTransakcijaViewModel
            {
                ProjektnaKartica = new ProjektnaKartica(),
                UlazneTransakcije = new List<Transakcija>(),
                IzlazneTransakcije = new List<Transakcija>(),
            };

            return View(viewModel);
        }


        /// <summary>
        /// Endpoint koji se poziva prilikom zahtjeva za stvaranje nove projektne kartice sa master-detail view-a.
        /// Provjerava ispravnost podataka i sprema novu projektnu karticu u bazu podataka zajedno sa transakcijama
        /// </summary>
        /// <param name="viewModel">ViewModel sa podacima o novoj projekt kartici</param>
        /// <returns>View sa detaljima o novoj projektnoj kartici</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create2(ProjektnaKarticaTransakcijaViewModel viewModel)
        {
            Console.WriteLine(viewModel.ToString(), "viewModel");
            var kartica = new ProjektnaKartica
            {
                Banka = viewModel.ProjektnaKartica.Banka,
                Stanje = viewModel.ProjektnaKartica.Stanje,
                Iban = viewModel.ProjektnaKartica.Iban,
                ProjektId = viewModel.ProjektnaKartica.ProjektId,
            };

            var result = validator.Validate(kartica);
            try
            {
                if (result.IsValid)
                {
                    _context.ProjektnaKartica.Add(kartica);
                    if (viewModel.UlazneTransakcije != null)
                    {
                        foreach (var transakcija in viewModel.UlazneTransakcije)
                        {
                            var newVrsta = _context.VrstaTransakcije.Find(transakcija.VrstaTransakcijeId);

                            var newTransakcija = new Transakcija();
                            newTransakcija.IbanIsporučitelja = transakcija.IbanIsporučitelja;
                            newTransakcija.IbanPrimatelja = kartica.Iban;
                            newTransakcija.Iznos = transakcija.Iznos;
                            newTransakcija.VrstaTransakcijeId = transakcija.VrstaTransakcijeId;
                            newTransakcija.ProjektnaKarticaPrimatelj = kartica;
                            newTransakcija.VrstaTransakcije = newVrsta;
                            newTransakcija.ProjektnaKarticaPrimateljId = kartica.ProjektnaKarticaId;
                            newTransakcija.ProjektnaKarticaIsporučiteljId = transakcija.ProjektnaKarticaIsporučiteljId;
                            _context.Transakcija.Add(newTransakcija);
                        }
                    }
                    if (viewModel.IzlazneTransakcije != null)
                    {
                        foreach (var transakcija in viewModel.IzlazneTransakcije)
                        {
                            var newVrsta = _context.VrstaTransakcije.Find(transakcija.VrstaTransakcijeId);

                            var newTransakcija = new Transakcija();
                            newTransakcija.IbanIsporučitelja = kartica.Iban;
                            newTransakcija.IbanPrimatelja = transakcija.IbanPrimatelja;
                            newTransakcija.Iznos = transakcija.Iznos;
                            newTransakcija.VrstaTransakcijeId = transakcija.VrstaTransakcijeId;
                            newTransakcija.ProjektnaKarticaIsporučitelj = kartica;
                            newTransakcija.VrstaTransakcije = newVrsta;
                            newTransakcija.ProjektnaKarticaIsporučiteljId = kartica.ProjektnaKarticaId;
                            newTransakcija.ProjektnaKarticaPrimateljId = transakcija.ProjektnaKarticaPrimateljId;
                            _context.Transakcija.Add(newTransakcija);
                        }
                    }
                    _context.SaveChanges();

                    TempData[Constants.ErrorOccurred] = false;
                    TempData["SuccessMessage"] = $"Projektna kartica {kartica.ProjektnaKarticaId} dodana.";
                    return RedirectToAction(nameof(Master2));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex.GetBaseException().Message);
                ModelState.AddModelError(string.Empty, ex.ToString());
                TempData[Constants.ErrorOccurred] = true;
                TempData["errorMessage"] = $"Projektnu karticu nije moguće dodati.";
                return View(viewModel);
            }
            TempData["errorMessage"] = $"Projektnu karticu nije moguće dodati.";
            return View(viewModel);
        }



        /// <summary>
        /// Endpoint koji se poziva prilikom pristupa Master view-u projektnih kartica, dohvaća sve projektne kartice i njihove transakcije iz baze podataka i prikazuje ih u View-u
        /// </summary>
        /// <param name="ViewType">Tip pogleda koji se želi prikazati</param>
        /// <param name="page">Broj stranice</param>
        /// <param name="sort">Po kojem atributu se želi sortirati</param>
        /// <param name="ascending">Vrst sortiranja (uzlazno/silazno)</param>
        /// <returns>View sa listom projektnih kartica</returns>
        public IActionResult Master2(string ViewType, int page = 1, int sort = 1, bool ascending = true)
        {
            int pageSize = 5;

            var kartice = _context.ProjektnaKartica.AsNoTracking();

            int count = kartice.Count();

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

            kartice = kartice
                .Include(z => z.Projekt)
            .Include(z => z.TransakcijaProjektnaKarticaIsporučitelj)
            .Include(z => z.TransakcijaProjektnaKarticaPrimatelj);


            var karticeSorted = kartice
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

            var model = new ProjektnaKarticaPageView
            {
                ProjektneKartice = karticeSorted,
                PagingInfo = pagingInfo
            };


            return View(ViewType, model);
        }

        /// <summary>
        /// Endpoint koji se poziva prilikom pristupa ekranu za uređivanje projektnih kartica sa master-detail view-a, dohvaća projektnu karticu i njene transackije iz baze podataka i prikazuje ju u View-u
        /// </summary>
        /// <param name="ProjektnaKarticaId">Id projektne kartice koja se želi uređivati</param>
        /// <returns>View sa detaljima o projektnoj kartici</returns>
        [HttpGet]
        public async Task<IActionResult> Edit2(int ProjektnaKarticaId)
        {
            var kartica = await _context.ProjektnaKartica
                                        .Include(p => p.Projekt)
                                        .Include(p => p.TransakcijaProjektnaKarticaPrimatelj)
                                        .Include(p => p.TransakcijaProjektnaKarticaIsporučitelj)
                                        .Where(p => p.ProjektnaKarticaId == ProjektnaKarticaId)
                                        .Select(p => new ProjektnaKarticaTransakcijaViewModel
                                        {
                                            ProjektnaKartica = p
                                        })
                                        .FirstOrDefaultAsync();
            if (kartica == null)
            {
                return NotFound($"Projektna kartica {ProjektnaKarticaId} ne postoji");
            }
            else
            {
                //učitavanje transakcija
                var ulazne = await _context.Transakcija
                                      .Include(e => e.VrstaTransakcije)
                                      .Where(e => e.ProjektnaKarticaPrimateljId == kartica.ProjektnaKartica.ProjektnaKarticaId)
                                      .OrderBy(e => e.TransakcijaId)
                                      .ToListAsync();

                kartica.UlazneTransakcije = ulazne;


                var izlazne = await _context.Transakcija
                                      .Include(e => e.VrstaTransakcije)
                                      .Where(e => e.ProjektnaKarticaIsporučiteljId == kartica.ProjektnaKartica.ProjektnaKarticaId)
                                      .OrderBy(e => e.TransakcijaId)
                                      .ToListAsync();

                kartica.IzlazneTransakcije = izlazne;

                return View(kartica);
            }
        }


        /// <summary>
        /// Endpoint koji se poziva prilikom zahtjeva za uređivanje projektnih kartica sa master-detail view-a, provjerava ispravnost podataka i sprema izmjenjenu projektnu karticu u bazu podataka
        /// </summary>
        /// <param name="viewModel">ViewModel sa podacima o izmjenjenoj projekt kartici</param>
        /// <param name="ProjektnaKarticaId">Id projektne kartice koja se želi uređivati</param>
        /// <returns>View sa detaljima o izmjenjenoj projektnoj kartici</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit2(ProjektnaKarticaTransakcijaViewModel viewModel, int ProjektnaKarticaId)
        {
            if (ModelState.IsValid)
            {
                var kartica = await _context.ProjektnaKartica
                                        .Include(p => p.Projekt)
                                        .Include(p => p.TransakcijaProjektnaKarticaIsporučitelj)
                                        .Include(p => p.TransakcijaProjektnaKarticaPrimatelj)
                                        .Where(p => p.ProjektnaKarticaId == ProjektnaKarticaId)
                                        .FirstOrDefaultAsync();

                kartica.Banka = viewModel.ProjektnaKartica.Banka;
                kartica.Stanje = viewModel.ProjektnaKartica.Stanje;
                kartica.Iban = viewModel.ProjektnaKartica.Iban;
                //kartica.ProjektId = viewModel.ProjektnaKartica.ProjektId;

                if (validator.Validate(kartica).IsValid)
                {
                    try
                    {
                        _context.Update(kartica);
                        await _context.SaveChangesAsync();

                        TempData[Constants.ErrorOccurred] = false;
                        TempData["SuccessMessage"] = $"Izmjene nad projektnom karticom {ProjektnaKarticaId} spremljene.";
                        return RedirectToAction(nameof(Edit2),
                                                       new { ProjektnaKarticaId = ProjektnaKarticaId });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception: {0}", ex.GetBaseException().Message);
                        ModelState.AddModelError(string.Empty, ex.ToString());
                        TempData[Constants.ErrorOccurred] = true;
                        TempData["errorMessage"] = $"Nije moguće spremiti izmjene nad projektnom karticom ={viewModel.ProjektnaKartica.ProjektnaKarticaId}.";

                        return View(viewModel);
                    }
                }
                else
                {
                    TempData[Constants.ErrorOccurred] = true;
                    TempData["errorMessage"] = $"Nije moguće spremiti izmjene nad projektnom karticom ={viewModel.ProjektnaKartica.ProjektnaKarticaId}.";
                    return View(viewModel);
                }
            }
            else
            {
                return View(viewModel);
            }
        }


        /// <summary>
        /// Endpoint koji se poziva prilikom zahtjeva za brisanje projektnih kartica sa master-detail view-a, provjerava postojanje projektnih kartica u bazi podataka i briše ih
        /// </summary>
        /// <param name="ProjektnaKarticaId">Id projektne kartice koja se želi obrisati</param>
        /// <param name="page">Broj stranice</param>
        /// <param name="sort">Atribut po kojem se želi sortirati</param>
        /// <param name="ascending">Vrst sortiranja (uzlazno/silazno)</param>
        /// <returns>View sa izmijenjenom listom projektnih kartica</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete2(int ProjektnaKarticaId, int page = 1, int sort = 1, bool ascending = true)
        {
            var karticaToBeDeleted = _context.ProjektnaKartica.Find(ProjektnaKarticaId);
            if (karticaToBeDeleted != null)
            {
                try
                {
                    _context.Remove(karticaToBeDeleted);
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = $"Projektna kartica {karticaToBeDeleted.ProjektnaKarticaId} uspješno obrisan.";
                }
                catch (Exception ex)
                {
                    TempData["errorMessage"] = $"Projektna kartica {karticaToBeDeleted.ProjektnaKarticaId} se ne može obrisati.";
                }
            }
            else
            {
                TempData["errorMessage"] = $"Projektna kartica {karticaToBeDeleted.ProjektnaKarticaId} se ne može obrisati.";
            }

            return RedirectToAction(nameof(Master2), new { page = page, sort = sort, ascending = ascending });
        }

        /*
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
        */

        /// <summary>
        /// Endpoint koji se poziva prilikom pristupa stranici za uvoz i izvoz podataka, prikazuje View za uvoz i izvoz podataka
        /// </summary>
        /// <returns>View za uvoz i izvoz podataka</returns>
        [HttpGet]
        public IActionResult UvozIzvoz()
        {
            return View();
        }

    }
}
