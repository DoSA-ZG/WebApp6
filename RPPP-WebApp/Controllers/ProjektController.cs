using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Data;
using RPPP_WebApp.Models;
using RPPP_WebApp.ModelsValidation;
using RPPP_WebApp.ViewModels;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.Intrinsics.X86;

namespace RPPP_WebApp.Controllers;

/// <summary>
/// Controller koji služi za obradu zahtjeva vezanih za projekt
/// </summary>
public class ProjektController : Controller
{

    private readonly RPPP06Context _context;

    /// <summary>
    /// Konstruktor razreda ProjektController.
    /// </summary>
    /// <param name="context">Kontekst baze podataka.</param>
    public ProjektController(RPPP06Context context)
    {

        _context = context;

    }

    /// <summary>
    /// Endpoint koji se poziva kako bi se pristupilo složenom tabličnom prikazu projekata, dohvaća sve projekte te ih obrađuje po predanim parametrima kako bi se mogli prikazati u složenom tabličnom prikazu u View-u
    /// </summary>
    /// <param name="page">Varijabla koja nam govori koja se stranica treba prikazati. Default vrijednost = 1</param>
    /// <param name="sort">Varijabla koja nam govori po kojoj će varijabli podatci na stranici biti sortirani. Default vrijednost = 1</param>
    /// <param name="ascending">Varijabla koja nam govori hoće li podaci biti sortirani uzlazno ili silazno. Default vrijednost = true</param>
    /// <returns>View sa listom svih projekata odabranih predanim parametrima</returns>
    public async Task<IActionResult> STP(int page = 1, int sort = 1, bool ascending = true)
    {

        int pageSize = 5;

        Dictionary<int, Func<Projekt, object>> sortOptions = new Dictionary<int, Func<Projekt, object>>
        {
            { 1, p => p.ProjektId },
            { 2, p => p.Ime },
            { 3, p => p.Kratica },
            { 4, p => p.Opis },
            { 5, p => p.PlaniraniPočetak },
            { 6, p => p.PlaniraniKraj },
            { 7, p => p.StvarniPočetak },
            { 8, p => p.StvarniKraj },
            { 9, p => p.VrstaProjekta.Ime },
            { 10, p => p.ProjektnaKartica.OrderBy(pk => pk.Iban).FirstOrDefault()?.Iban },
            { 11, p => p.Dokumentacija.OrderBy(d => d.NazivDokumentacije).FirstOrDefault()?.NazivDokumentacije },
            { 12, p => p.PlanProjekta?.PlanProjektaId },
            { 13, p => p.SuradnikEmail.OrderBy(se => se.Email).FirstOrDefault()?.Email },
        };

        List<Projekt> sviProjekti = await _context.Projekt
            .Include(s => s.SuradnikEmail)
            .Include(pp => pp.PlanProjekta)
            .Include(d => d.Dokumentacija)
            .Include(pk => pk.ProjektnaKartica)
            .Include(vp => vp.VrstaProjekta)
            .ToListAsync();

        int projektCount = sviProjekti.Count;
        sviProjekti = ascending ? sviProjekti.OrderBy(sortOptions[sort]).ToList() : sviProjekti.OrderByDescending(sortOptions[sort]).ToList();
        sviProjekti = sviProjekti.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        ViewBag.Page = page;
        ViewBag.TotalPageCount = (int)Math.Ceiling((decimal)projektCount / pageSize);
        ViewBag.Sort = sort;
        ViewBag.Ascending = ascending;

        return View(sviProjekti);
    }


    /// <summary>
    /// Endpoint koji nam vraća view za kreiranje novog projekta
    /// </summary>
    /// <returns>View koji sadrži view model koji sadrži projekt sa default vrijednostima te odabirom vrste projekta za radio-buttone</returns>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        List<VrstaProjekta> vrsteProjekta = _context.VrstaProjekta.ToList();
        var viewModel = new ProjektViewModel
        {
            projekt = new Projekt(),
            SelectedVrstaProjekt = null
        };
        ViewBag.VrsteProjektaList = vrsteProjekta;
        viewModel.projekt.PlaniraniPočetak = DateTime.Now;
        viewModel.projekt.PlaniraniKraj = DateTime.Now;
        return View(viewModel);
    }

    /// <summary>
    /// Endpoint koji služi za obradu post zahtjeva za kreaciju novog projekta. Evaluira dane podatke te ukoliko je dan projekt ispravan kreira novi projekt u bazi podataka.
    /// </summary>
    /// <param name="projektViewModel">Model koji sadrži podatke potrebne za kreaciju novog projekta</param>
    /// <returns>View koji sadrži view model koji smo dobili kao parametar kod pozivanja endpointa ili redirect na ManageTable view ovisno o tome prođe li projekt validaciju</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ProjektViewModel projektViewModel)
    {
        if (projektViewModel.projekt.PlaniraniPočetak > projektViewModel.projekt.PlaniraniKraj)
        {
            ModelState.AddModelError("projekt.PlaniraniPočetak", "Datum početka mora biti prije datuma stvarnog kraja.");
        }

        if (projektViewModel.projekt.StvarniPočetak > projektViewModel.projekt.StvarniKraj)
        {
            ModelState.AddModelError("projekt.StvarniPočetak", "Datum stvarnog početka mora biti prije datuma stvarnog kraja.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var vrstaProjekta = _context.VrstaProjekta.FirstOrDefault(p => p.Ime == projektViewModel.SelectedVrstaProjekt);
                projektViewModel.projekt.VrstaProjekta = vrstaProjekta;
                _context.Projekt.Add(projektViewModel.projekt);
                _context.SaveChanges();
                return RedirectToAction("ManageTable");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while saving Projekt.");
            }
        }

        List<VrstaProjekta> vrsteProjekta = _context.VrstaProjekta.ToList();
        ViewBag.VrsteProjektaList = vrsteProjekta;
        return View(projektViewModel);
    }
    
    /// <summary>
    /// Endpoint za brisanje projekta, provjerava postoji li projekt te ga briše ukoliko postoji
    /// </summary>
    /// <param name="id">Varijabla koja označava identifikator projekta kojeg želimo obrisati</param>
    /// <returns>Redirect na view ManageTable</returns>
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var projekt = await _context.Projekt.FindAsync(id);

        if (projekt == null)
        {
            return NotFound();
        }

        try
        {
            _context.Projekt.Remove(projekt);
            await _context.SaveChangesAsync();

            return RedirectToAction("ManageTable");
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error");
        }
    }

    /// <summary>
    /// Endpoint koji se poziva kako bi se pristupilo tabličnom prikazu projekata koji ima gumbe za brisanje i pristupanje master detail formi. Dohvaća sve projekte te ih obrađuje po predanim parametrima kako bi se mogli prikazati tabličnom prikazu u View-u
    /// </summary>
    /// <param name="page">Varijabla koja nam govori koja se stranica treba prikazati. Default vrijednost = 1</param>
    /// <param name="sort">Varijabla koja nam govori po kojoj će varijabli podatci na stranici biti sortirani. Default vrijednost = 1</param>
    /// <param name="ascending">Varijabla koja nam govori hoće li podaci biti sortirani uzlazno ili silazno. Default vrijednost = true</param>
    /// <returns>View sa listom svih projekata odabranih predanim parametrima</returns>
    [HttpGet]
    public async Task<IActionResult> ManageTable(int page = 1, int sort = 1, bool ascending = true)
    {
        int pageSize = 5;

        Dictionary<int, Func<Projekt, object>> sortOptions = new Dictionary<int, Func<Projekt, object>>
        {
            { 1, p => p.ProjektId },
            { 2, p => p.Ime },
            { 3, p => p.Kratica },
            { 4, p => p.Opis },
            { 5, p => p.PlaniraniPočetak },
            { 6, p => p.PlaniraniKraj },
            { 7, p => p.StvarniPočetak },
            { 8, p => p.StvarniKraj },
            { 9, p => p.VrstaProjekta.Ime },
            { 10, p => p.ProjektnaKartica.OrderBy(pk => pk.Iban).FirstOrDefault()?.Iban },
            { 11, p => p.Dokumentacija.OrderBy(d => d.NazivDokumentacije).FirstOrDefault()?.NazivDokumentacije },
            { 12, p => p.PlanProjekta?.PlanProjektaId },
            { 13, p => p.SuradnikEmail.OrderBy(se => se.Email).FirstOrDefault()?.Email },
        };

        List<Projekt> sviProjekti = await _context.Projekt
            .Include(s => s.SuradnikEmail)
            .Include(pp => pp.PlanProjekta)
            .Include(d => d.Dokumentacija)
            .Include(pk => pk.ProjektnaKartica)
            .Include(vp => vp.VrstaProjekta)
            .ToListAsync();

        int dokumentacijaCount = sviProjekti.Count;
        sviProjekti = ascending ? sviProjekti.OrderBy(sortOptions[sort]).ToList() : sviProjekti.OrderByDescending(sortOptions[sort]).ToList();
        sviProjekti = sviProjekti.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        ViewBag.Page = page;
        ViewBag.TotalPageCount = (int)Math.Ceiling((decimal)dokumentacijaCount / pageSize);
        ViewBag.Sort = sort;
        ViewBag.Ascending = ascending;

        return View(sviProjekti);
    }

    /// <summary>
    /// Endpoint koji nam služi za dohvaćanje odabranog projekta kako bi se prikazale njegove vrijednosti radi uređivanja. Uz podatke projekta dohvaćaju se i svi vezani entiteti radi prikaza.
    /// </summary>
    /// <param name="id">Varijabla koja označava identifikator projekta kojeg želimo uređivati</param>
    /// <returns>View koji sadrži view model koi u sebi sadrži projekt te varijablu za odabir vrste projekta koja u view-u služi za radio buttone</returns>
    [HttpGet]
    public async Task<IActionResult> Manage(int id)
    {
        List<VrstaProjekta> vrsteProjekta = _context.VrstaProjekta.ToList();
        Dictionary<int, string> vrstaDokumentacije = _context.VrstaDokumentacije.ToDictionary(vp => vp.VrstaDokumentacijeId, vp => vp.Ime);
        var viewModel = new ProjektViewModel
        {
            projekt = _context.Projekt
            .Include(s => s.SuradnikEmail)
            .Include(pp => pp.PlanProjekta)
            .Include(d => d.Dokumentacija)
            .Include(pk => pk.ProjektnaKartica)
            .FirstOrDefault(p => p.ProjektId == id),
            SelectedVrstaProjekt = null
        };
        viewModel.SelectedVrstaProjekt = _context.VrstaProjekta.FirstOrDefault(vd => vd.VrstaProjektaId == viewModel.projekt.VrstaProjektaId).Ime;
        ViewBag.VrsteProjektaList = vrsteProjekta;
        ViewBag.vrstaDokumentacije = vrstaDokumentacije;

        return View(viewModel);
    }

    /// <summary>
    /// Endpoint koji provjerava postoji li predani projekt te prolazi li sve validacije. Ukoliko postoji i zadovoljava sve evaluacije mijenja njegove podatke.
    /// </summary>
    /// <param name="id">Varijabla koja slući kao identifikator projekta kojeg želimo izmjeniti</param>
    /// <param name="projektViewModel">View model koji u sebi sadrži izmjene koje postojeći projekt treba poprimiti</param>
    /// <returns>View koji sadrži view model koji smo dobili kao parametar kod pozivanja endpointa ili redirect na ManageTable view i listu svih projekata ovisno o tome prođe li projekt validaciju</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Manage(int id, ProjektViewModel projektViewModel)
    {

        if (projektViewModel.projekt.PlaniraniPočetak > projektViewModel.projekt.PlaniraniKraj)
        {
            ModelState.AddModelError("projekt.PlaniraniPočetak", "Datum početka mora biti prije datuma stvarnog kraja.");
        }

        if (projektViewModel.projekt.StvarniPočetak > projektViewModel.projekt.StvarniKraj)
        {
            ModelState.AddModelError("projekt.StvarniPočetak", "Datum stvarnog početka mora biti prije datuma stvarnog kraja.");
        }

        if (ModelState.IsValid)
        {
            try
            {

                var projekt = _context.Projekt.Find(id);

                if (projekt == null)
                {
                    return NotFound();
                }

                var vrstaProjekta = _context.VrstaProjekta.FirstOrDefault(p => p.Ime == projektViewModel.SelectedVrstaProjekt);
                projekt.Ime = projektViewModel.projekt.Ime;
                projekt.Kratica = projektViewModel.projekt.Kratica;
                projekt.Opis = projektViewModel.projekt.Opis;
                projekt.PlaniraniPočetak = projektViewModel.projekt.PlaniraniPočetak;
                projekt.PlaniraniKraj = projektViewModel.projekt.PlaniraniKraj;
                projekt.StvarniPočetak = projektViewModel.projekt.StvarniPočetak;
                projekt.StvarniKraj = projektViewModel.projekt.StvarniKraj;
                projekt.VrstaProjektaId = vrstaProjekta.VrstaProjektaId;
                _context.SaveChanges();

                List<Projekt> sviProjekti = _context.Projekt
                    .Include(s => s.SuradnikEmail)
                    .Include(pp => pp.PlanProjekta)
                    .Include(d => d.Dokumentacija)
                    .Include(pk => pk.ProjektnaKartica)
                    .Include(vp => vp.VrstaProjekta)
                    .ToList();

                int pageSize = 5;
                int projektCount = sviProjekti.Count;
                sviProjekti = sviProjekti.OrderBy(p => p.ProjektId).ToList();
                sviProjekti = sviProjekti.Take(pageSize).ToList();
                ViewBag.Page = 1;
                ViewBag.Sort = 1;
                ViewBag.Ascending = true;
                ViewBag.TotalPageCount = (int)Math.Ceiling((decimal)projektCount / pageSize);

                return View("ManageTable", sviProjekti);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while saving Projekt.");
            }
        }

        List<VrstaProjekta> vrsteProjekta = _context.VrstaProjekta.ToList();
        ViewBag.VrsteProjektaList = vrsteProjekta;
        return View(projektViewModel);
    }

    /// <summary>
    /// Endpoint koji služi za prikaz detalja projekta ukoliko postoji
    /// </summary>
    /// <param name="id">Varijabla koja služi kao identifikator projekta za kojeg želimo prikazati view</param>
    /// <returns>View sa viewModel-om koji sadrži projekt i varijablu u koju će biti pospremljena vrsta projekta kod radio-buttona</returns>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {

        Dictionary<int, string> vrstaProjekta = _context.VrstaProjekta.ToDictionary(vp => vp.VrstaProjektaId, vp => vp.Ime);
        Dictionary<int, string> vrstaDokumentacije = _context.VrstaDokumentacije.ToDictionary(vd => vd.VrstaDokumentacijeId, vd => vd.Ime);
        var viewModel = new ProjektViewModel
        {
            projekt = _context.Projekt
            .Include(s => s.SuradnikEmail)
            .Include(pp => pp.PlanProjekta)
            .Include(d => d.Dokumentacija)
            .Include(pk => pk.ProjektnaKartica)
            .FirstOrDefault(p => p.ProjektId == id),
            SelectedVrstaProjekt = null
        };
        viewModel.SelectedVrstaProjekt = _context.VrstaProjekta.FirstOrDefault(vd => vd.VrstaProjektaId == viewModel.projekt.VrstaProjektaId).Ime;
        ViewBag.VrsteProjektaList = vrstaProjekta;
        ViewBag.vrstaDokumentacije = vrstaDokumentacije;

        return View(viewModel);
    }

    /// <summary>
    /// Endpoint koji služi za prikaz sljedećeg projekta u master detail formi
    /// </summary>
    /// <param name="id">Varijabla koji služi kao identifikator projekta za kojeg želimo prikazati sljedeći projekt</param>
    /// <returns>View Manage te viewModel koji sadrži podatke o sljedećem projektu kojeg želimo prikazati</returns>
    [HttpGet]
    public async Task<IActionResult> ManageNext(int id)
    {
        try
        {
            var currentProject = await _context.Projekt
                .OrderBy(p => p.ProjektId)
                .FirstOrDefaultAsync(p => p.ProjektId == id);

            if (currentProject == null)
            {
                return NotFound();
            }

            var nextProject = await _context.Projekt
                .OrderBy(p => p.ProjektId)
                .Include(s => s.SuradnikEmail)
                .Include(pp => pp.PlanProjekta)
                .Include(d => d.Dokumentacija)
                .Include(pk => pk.ProjektnaKartica)
                .FirstOrDefaultAsync(p => p.ProjektId > id);

            if (nextProject == null)
            {
                nextProject = await _context.Projekt
                    .OrderBy(p => p.ProjektId)
                    .Include(s => s.SuradnikEmail)
                    .Include(pp => pp.PlanProjekta)
                    .Include(d => d.Dokumentacija)
                    .Include(pk => pk.ProjektnaKartica)
                    .FirstOrDefaultAsync();

                if (nextProject == null)
                {
                    return RedirectToAction("ManageTable");
                }
            }

            List<VrstaProjekta> vrsteProjekta = _context.VrstaProjekta.ToList();
            Dictionary<int, string> vrstaDokumentacije = _context.VrstaDokumentacije.ToDictionary(vp => vp.VrstaDokumentacijeId, vp => vp.Ime);
            var viewModel = new ProjektViewModel
            {
                projekt = nextProject,
                SelectedVrstaProjekt = _context.VrstaProjekta.FirstOrDefault(vd => vd.VrstaProjektaId == nextProject.VrstaProjektaId)?.Ime
            };

            ViewBag.VrsteProjektaList = vrsteProjekta;
            ViewBag.vrstaDokumentacije = vrstaDokumentacije;

            return View("Manage", viewModel);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred while retrieving the next Projekt.");
            return RedirectToAction("Error"); 
        }
    }

    /// <summary>
    /// Endpoint koji služi za prikaz prošlog projekta u master detail formi
    /// </summary>
    /// <param name="id">Varijabla koji služi kao identifikator projekta za kojeg želimo prikazati prošli projekt</param>
    /// <returns>View Manage te viewModel koji sadrži podatke o prošlom projektu kojeg želimo prikazati</returns>
    [HttpGet]
    public async Task<IActionResult> ManagePrev(int id)
    {
        try
        {
            var currentProject = await _context.Projekt
                .OrderBy(p => p.ProjektId)
                .FirstOrDefaultAsync(p => p.ProjektId == id);

            if (currentProject == null)
            {
                return NotFound();
            }

            var prevProject = await _context.Projekt
                .OrderByDescending(p => p.ProjektId)
                .Include(s => s.SuradnikEmail)
                .Include(pp => pp.PlanProjekta)
                .Include(d => d.Dokumentacija)
                .Include(pk => pk.ProjektnaKartica)
                .FirstOrDefaultAsync(p => p.ProjektId < id);

            if (prevProject == null)
            {
                prevProject = await _context.Projekt
                    .OrderByDescending(p => p.ProjektId)
                    .Include(s => s.SuradnikEmail)
                    .Include(pp => pp.PlanProjekta)
                    .Include(d => d.Dokumentacija)
                    .Include(pk => pk.ProjektnaKartica)
                    .FirstOrDefaultAsync();

                if (prevProject == null)
                {
                    return RedirectToAction("ManageTable");
                }
            }

            List<VrstaProjekta> vrsteProjekta = _context.VrstaProjekta.ToList();
            Dictionary<int, string> vrstaDokumentacije = _context.VrstaDokumentacije.ToDictionary(vp => vp.VrstaDokumentacijeId, vp => vp.Ime);
            var viewModel = new ProjektViewModel
            {
                projekt = prevProject,
                SelectedVrstaProjekt = _context.VrstaProjekta.FirstOrDefault(vd => vd.VrstaProjektaId == prevProject.VrstaProjektaId)?.Ime
            };

            ViewBag.VrsteProjektaList = vrsteProjekta;
            ViewBag.vrstaDokumentacije = vrstaDokumentacije;

            return View("Manage", viewModel);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred while retrieving the previous Projekt.");
            return RedirectToAction("Error");
        }
    }

}
