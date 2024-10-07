using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NLog.Fluent;
using RPPP_WebApp.Data;
using RPPP_WebApp.Exstensions.Selectors;
using RPPP_WebApp.Models;
using RPPP_WebApp.ModelsValidation;
using RPPP_WebApp.ViewModels;


namespace RPPP_WebApp.Controllers
{
    public class PosaoController : Controller
    {

        private readonly RPPP06Context _context;
        private readonly AppSettings appSettings;
        private readonly ILogger<PosaoController> logger;
        PosaoValidator posaoValidator = new PosaoValidator();

        public PosaoController(RPPP06Context context, IOptionsSnapshot<AppSettings> options, ILogger<PosaoController> logger)
        {
            _context = context;
            appSettings = options.Value;
            this.logger = logger;
        }
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var objPosao = _context.Posao.AsNoTracking();          
                                                           
 

            int count = objPosao.Count();
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
            objPosao = objPosao.Include(p => p.VrstaPosla)
                                                            .Include(p => p.SuradnikEmailNavigation)
                                                            .Include(p => p.Zadatak)
                                                            .ThenInclude(z => z.Zahtjev).ThenInclude(z => z.PlanProjekta).ThenInclude(z => z.Projekt).ApplySort(sort, ascending); ;

            var objPosaoList = objPosao
                        .Skip((page - 1) * pagesize)
                        .Take(pagesize)

                        .ToList();
            var model = new PosaoPageView
            {
                Posao = objPosaoList,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            Posao posao = new Posao();
            await PrepareDropDownListsVrstaPosla();
            await PrepareDropDownListsSuradnici();
            await PrepareDropDownListsZadatak();
            var viewModel = new PosaoViewModel
            {
                posao = posao,
                zadatak = null,
                vrstaPosla = null
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PosaoViewModel posaoview)
        {

            Posao posao1 = posaoview.posao;

            var rezultatValidacije = posaoValidator.Validate(posao1);

            if (!rezultatValidacije.IsValid)
            {
                foreach (var error in rezultatValidacije.Errors)
                {
                    Console.WriteLine($"Property: {error.PropertyName}, Error: {error.ErrorMessage}.");
                }

            }

            if (rezultatValidacije.IsValid)
            {
                try
                {
                    var posao = new Posao
                    {
                        Opis = posaoview.posao.Opis,
                        VrijemePočetkaRada = posaoview.posao.VrijemePočetkaRada,
                        VrijemeKrajaRada = posaoview.posao.VrijemeKrajaRada,
                        SuradnikEmail = posaoview.posao.SuradnikEmail
                        // Set other properties as needed
                    };

                    var zadatakId = _context.Zadatak
                        .Where(x => x.OpisZadatka == posaoview.zadatak)
                                .Select(x => x.ZadatakId)
                                .FirstOrDefault();
                    posao.ZadatakId= zadatakId;
                    var vrstaPosla = _context.VrstaPosla.Where(x => x.Ime == posaoview.vrstaPosla)
                                .Select(x => x.VrstaPoslaId)
                                .FirstOrDefault();
                    posao.VrstaPoslaId = vrstaPosla;
                     _context.Posao.Add(posao);
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = $"Posao {posao.Opis} uspješno dodan.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Handle the exception, log it, or display an error message
                    TempData["errorMessage"] = $"Posao {posaoview.posao.Opis} uspješno dodan.";
                    // You may want to return to the same view to display the error.
                }
            }
            await PrepareDropDownListsVrstaPosla();
            await PrepareDropDownListsSuradnici();
            await PrepareDropDownListsZadatak();
            TempData["errorMessage"] = $"Posao {posaoview.posao.Opis} nije moguće dodati.";
            TempData["errorMessage"] = $"{rezultatValidacije}";
            return View(posaoview);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int Id, int page = 1, int sort = 1, bool ascending = true)
        {
            var posao = _context.Posao.AsNoTracking().Include(d => d.Zadatak).Include(d => d.VrstaPosla).Where(d => d.PosaoId == Id).SingleOrDefault();
            if (posao == null)
            {
               
                return NotFound("Ne postoji suradnik s oznakom: " + Id);
            }
            else
            {

                await PrepareDropDownListsVrstaPosla();
                await PrepareDropDownListsSuradnici();
                await PrepareDropDownListsZadatak();
                var viewModel = new PosaoViewModel
                {
                    posao = posao,
                    zadatak = posao.Zadatak.OpisZadatka,
                    vrstaPosla = posao.VrstaPosla.Ime
                };
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                return View(viewModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(PosaoViewModel posaoView, int page = 1, int sort = 1, bool ascending = true)
        {

            var existingPosao = await _context.Posao
                   .Include(d => d.Zadatak).Include(d => d.VrstaPosla)
                   .FirstOrDefaultAsync(s => s.PosaoId == posaoView.posao.PosaoId);
            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;
            if (ModelState.IsValid)
            {
                try
                {
                    // Retrieve the existing Suradnik from the database

                    if (existingPosao == null)
                    {
                        return NotFound("Suradnik nije pronađen");
                    }

                    // Update properties of the existing Suradnik with data from the view model
                    //existingSuradnik.Email = suradnikView.suradnik.Email;
                    existingPosao.Opis = posaoView.posao.Opis;
                    existingPosao.VrijemePočetkaRada = posaoView.posao.VrijemePočetkaRada;
                    existingPosao.VrijemeKrajaRada = posaoView.posao.VrijemeKrajaRada;
                    existingPosao.SuradnikEmail = posaoView.posao.SuradnikEmail;

                    var zadatakId = _context.Zadatak
                       .Where(x => x.OpisZadatka == posaoView.zadatak)
                               .Select(x => x.ZadatakId)
                               .FirstOrDefault();
                    existingPosao.ZadatakId = zadatakId;
                    var vrstaPosla = _context.VrstaPosla.Where(x => x.Ime == posaoView.vrstaPosla)
                                .Select(x => x.VrstaPoslaId)
                                .FirstOrDefault();
                    existingPosao.VrstaPoslaId = vrstaPosla;



                    // Save changes to persist the updates
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Posao {existingPosao.Opis} uspješno promijenjen.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Handle the exception, log it, or display an error message
                    TempData["errorMessage"] = $"Posao {existingPosao.Opis} neuspješno promijenjen.";
                    // You may want to return to the same view to display the error.
                    await PrepareDropDownListsVrstaPosla();
                    await PrepareDropDownListsSuradnici();
                    await PrepareDropDownListsZadatak();
                    return View(posaoView);
                }
            }
            await PrepareDropDownListsVrstaPosla();
            await PrepareDropDownListsSuradnici();
            await PrepareDropDownListsZadatak();
            return View(posaoView);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int Id, int page = 1, int sort = 1, bool ascending = true)
        {
            var posao = _context.Posao.Find(Id);

            if (posao != null)
            {
                try
                {
                    _context.Remove(posao);
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = $"Posao {posao.Opis} uspješno obrisan.";
                }
                catch (Exception ex)
                {
                    //Greške TempData i logger
                    TempData["errorMessage"] = $"Posao {posao.Opis} nije moguće obrisati.";
                }
            }
            else
            {
                //Greške TempData i logger
            }
            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
        }



        private async Task PrepareDropDownListsSuradnici()
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

        private async Task PrepareDropDownListsZadatak()
        {

            var zadatak = await _context.Zadatak
                          .OrderBy(d => d.OpisZadatka)
                          .Select(d => new SelectListItem
                          {
                              Value = d.OpisZadatka,  // Set the appropriate property as the value
                              Text = $"{d.OpisZadatka} ({d.Zahtjev.PlanProjekta.Projekt.Ime})"  // Set the appropriate properties as the text
                          })
                          .ToListAsync();

            ViewBag.zadatak = new SelectList(zadatak, nameof(SelectListItem.Value), nameof(SelectListItem.Text));

        }
    }
}
