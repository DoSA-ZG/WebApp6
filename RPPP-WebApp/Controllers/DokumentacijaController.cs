using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Data;
using RPPP_WebApp.Models;
using RPPP_WebApp.ModelsValidation;
using RPPP_WebApp.ViewModels;
using System.Drawing.Printing;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace RPPP_WebApp.Controllers;

/// <summary>
/// Controller koji služi za obradu zahtjeva vezanih za dokumentaciju
/// </summary>
public class DokumentacijaController : Controller
{

    private readonly RPPP06Context _context;

    /// <summary>
    /// Konstruktor razreda DokumentacijaController.
    /// </summary>
    /// <param name="context">Kontekst baze podataka.</param>
    public DokumentacijaController(RPPP06Context context)
    {

        _context = context;

    }

    /// <summary>
    /// Endpoint koji se poziva kako bi se pristupilo jednostavnom tabličnom prikazu dokumentacije, dohvaća svu dokumentaciju te je obrađuje po predanim parametrima kako bi se mogla prikazati u jednostavnom tabličnom prikazu u View-u
    /// </summary>
    /// <param name="page">Varijabla koja nam govori koja se stranica treba prikazati. Default vrijednost = 1</param>
    /// <param name="sort">Varijabla koja nam govori po kojoj će varijabli podatci na stranici biti sortirani. Default vrijednost = 1</param>
    /// <param name="ascending">Varijabla koja nam govori hoće li podaci biti sortirani uzlazno ili silazno. Default vrijednost = true</param>
    /// <returns>View sa listom svih dokumenata odabranih predanim parametrima</returns>
    [HttpGet]
    public async Task<IActionResult> JTP(int page = 1, int sort = 1, bool ascending = true)
    {
        int pageSize = 5;

        Dictionary<int, Func<Dokumentacija, object>> sortOptions = new Dictionary<int, Func<Dokumentacija, object>>
    {
        { 1, d => d.DokumentacijaId },
        { 2, d => d.NazivDokumentacije },
        { 3, d => d.VrijemeKreacije },
        { 4, d => d.Format },
        { 5, d => d.URL },
        { 6, d => d.StatusDovršenosti },
        { 7, d => d.Projekt.Ime },
        { 8, d => d.VrstaDokumentacije.Ime }
    };

        List<Dokumentacija> dokumentacija = await _context.Dokumentacija
            .Include(p => p.Projekt)
            .Include(vd => vd.VrstaDokumentacije)
            .ToListAsync();
        int dokumentacijaCount = dokumentacija.Count;
        dokumentacija = ascending ? dokumentacija.OrderBy(sortOptions[sort]).ToList() : dokumentacija.OrderByDescending(sortOptions[sort]).ToList();
        dokumentacija = dokumentacija.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        ViewBag.Page = page;
        ViewBag.TotalPageCount = (int) Math.Ceiling((decimal) dokumentacijaCount / pageSize);
        ViewBag.Sort = sort;
        ViewBag.Ascending = ascending;

        return View(dokumentacija);
    }

    /// <summary>
    /// Endpoint koji nam vraća view za kreiranje nove dokumentacije
    /// </summary>
    /// <returns>View koji sadrži viewModel koji sadrži dokumentaciju sa default vrijednostima te odabirom vrste i statusa dokumentacije za radio-buttone</returns>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        List<VrstaDokumentacije> vrsteDokumentacije = _context.VrstaDokumentacije.ToList();
        var viewModel = new DokumentacijaViewModel
        {
            dokumentacija = new Dokumentacija { },
            SelectedVrstaDokumentacije = null,
            SelectedStatusDokumentacije = null

        };
        ViewBag.VrsteDokumentacije = vrsteDokumentacije;
        var projekti = await _context.Projekt
                      .OrderBy(p => p.Ime)
                      .Select(p => new SelectListItem
                      {
                          Value = p.ProjektId.ToString(),
                          Text = $"{p.Ime}, {p.Kratica}"
                      })
                      .ToListAsync();
        ViewBag.Projekti = new SelectList(projekti, nameof(SelectListItem.Value), nameof(SelectListItem.Text));

        return View(viewModel);
    }

    /// <summary>
    /// Endpoint koji služi za obradu post zahtjeva za kreaciju novog dokumenta. Evaluira dane podatke te ukoliko je dana dokumentacija ispravna kreira novi dokument u bazi podataka.
    /// </summary>
    /// <param name="dokumentacijaViewModel">Model koji sadrži podatke potrebne za kreaciju nove dokumentacije</param>
    /// <returns>Redirect na JTP view ili vraćeni dokumentacijaViewModel koji nam je predan pri pozivu funkcije ovisno o tome prođe li dokument validaciju</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(DokumentacijaViewModel dokumentacijaViewModel)
    {
        try
        {

            Dokumentacija dokumentacijaZaSpremiti = new Dokumentacija();

            dokumentacijaZaSpremiti = dokumentacijaViewModel.dokumentacija;

            dokumentacijaZaSpremiti.VrijemeKreacije = DateTime.Now;

            dokumentacijaZaSpremiti.StatusDovršenosti = dokumentacijaViewModel.SelectedStatusDokumentacije;

            dokumentacijaZaSpremiti.DokumentacijaId = dokumentacijaViewModel.dokumentacija.DokumentacijaId;

            var vrstaDokumentacijeId = _context.VrstaDokumentacije.FirstOrDefault(d => d.Ime == dokumentacijaViewModel.SelectedVrstaDokumentacije);
            dokumentacijaZaSpremiti.VrstaDokumentacijeId = vrstaDokumentacijeId.VrstaDokumentacijeId;



            _context.Dokumentacija.Add(dokumentacijaZaSpremiti);
            _context.SaveChanges();
            return RedirectToAction("JTP");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred while saving Dokumentacija.");
        }
        return View(dokumentacijaViewModel);
    }

    /// <summary>
    /// Endpoint za brisanje dokumentacije, provjerava postoji li dokumentacija te ga briše ukoliko postoji
    /// </summary>
    /// <param name="Id">Varijabla koja označava identifikator dokumenta kojeg želimo obrisati</param>
    /// <returns>Redirect na JTP view</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int Id)
    {
        try
        {
            var dokumentacija = _context.Dokumentacija.Find(Id);

            if (dokumentacija == null)
            {
                return NotFound();
            }

            _context.Dokumentacija.Remove(dokumentacija);
            _context.SaveChanges();
            return RedirectToAction("JTP");
        }
        catch (Exception ex)
        {
            // Handle exception
            return RedirectToAction("JTP");
        }
    }

    /// <summary>
    /// Endpoint koji služi za pristup Edit view-u dokumenta
    /// </summary>
    /// <param name="Id">Varijabla koja služi kao identifikator dokumentacije koju želimo urediti</param>
    /// <returns>View koji sadrži view model koi u sebi sadrži dokument te varijablu za odabir vrste dokumenta i status koji u view-u služe za radio buttone</returns>
    [HttpGet]
    public async Task<IActionResult> Edit(int Id)
    {
        Dokumentacija exsitingDokumentacija = _context.Dokumentacija.FirstOrDefault(d => d.DokumentacijaId == Id);
        List<VrstaDokumentacije> vrsteDokumentacije = _context.VrstaDokumentacije.ToList();
        var viewModel = new DokumentacijaViewModel
        {
            dokumentacija = exsitingDokumentacija,
            SelectedVrstaDokumentacije = _context.VrstaDokumentacije.FirstOrDefault(vd => vd.VrstaDokumentacijeId == exsitingDokumentacija.VrstaDokumentacijeId).Ime,
            SelectedStatusDokumentacije = exsitingDokumentacija.StatusDovršenosti,
            SelectedProjekt = _context.Projekt.FirstOrDefault(p => p.ProjektId == exsitingDokumentacija.ProjektId)
        };
        ViewBag.VrsteDokumentacije = vrsteDokumentacije;
        var projekti = await _context.Projekt
                      .OrderBy(p => p.Ime)
                      .Select(p => new SelectListItem
                      {
                          Value = p.ProjektId.ToString(),
                          Text = $"{p.Ime}, {p.Kratica}"
                      })
                      .ToListAsync();

        ViewBag.Projekti = new SelectList(projekti, nameof(SelectListItem.Value), nameof(SelectListItem.Text));

        return View(viewModel);
    }

    /// <summary>
    /// Endpoint koji provjerava postoji li predani dokument te prolazi li sve validacije. Ukoliko postoji i zadovoljava sve evaluacije mijenja njegove podatke.
    /// </summary>
    /// <param name="id">Varijabla koja slući kao identifikator dokumenta kojeg želimo izmjeniti</param>
    /// <param name="dokumentacijaViewModel">View model koji u sebi sadrži izmjene koje postojeći dokument treba poprimiti</param>
    /// <returns>Redirect na JTP view</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, DokumentacijaViewModel dokumentacijaViewModel)
    {
        try
        {
            var dokumentacija = _context.Dokumentacija.Include(vd => vd.VrstaDokumentacije).FirstOrDefault(d => d.DokumentacijaId == id);

            if (dokumentacija == null)
            {
                return NotFound();
            }

            dokumentacija.NazivDokumentacije = dokumentacijaViewModel.dokumentacija.NazivDokumentacije;
            dokumentacija.VrijemeKreacije = DateTime.Now;
            dokumentacija.Format = dokumentacijaViewModel.dokumentacija.Format;
            dokumentacija.URL = dokumentacijaViewModel.dokumentacija.URL;
            var vrstaDokumentacije = _context.VrstaDokumentacije.FirstOrDefault(d => d.Ime == dokumentacijaViewModel.SelectedVrstaDokumentacije);
            dokumentacija.VrstaDokumentacijeId = vrstaDokumentacije.VrstaDokumentacijeId;
            dokumentacija.StatusDovršenosti = dokumentacijaViewModel.SelectedStatusDokumentacije;

            _context.SaveChanges();

            return RedirectToAction("JTP");
        }
        catch (Exception ex)
        {
            // Handle exception
            return RedirectToAction("JTP");
        }
    }

    /// <summary>
    /// Endpoint koji služi za prikaz detalja dokumenta ukoliko postoji
    /// </summary>
    /// <param name="Id">Varijabla koja služi kao identifikator dokumenta za kojeg želimo prikazati view</param>
    /// <returns>View sa viewModel-om koji sadrži dokument  i varijablu u koju će biti pospremljene vrste dokumenta i status kod radio-buttona</returns>
    [HttpGet]
    public async Task<IActionResult> Details(int Id)
    {
        var viewModel = new DokumentacijaViewModel
        {
            dokumentacija = _context.Dokumentacija.Include(p => p.Projekt).FirstOrDefault(d => d.DokumentacijaId == Id),
            SelectedVrstaDokumentacije = null,
            SelectedStatusDokumentacije = null

        };

        return View(viewModel);
    }

    /// <summary>
    /// Endpoint koji nam vraća Jtable view
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> JTable()
    {
        return View();
    }
}
