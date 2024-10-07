using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Data;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.ModelsValidation;
using System.Linq;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.Controllers
{

    public class TransakcijaController(RPPP06Context context) : Controller
    {
        private readonly RPPP06Context _context = context;
        TransakcijaValidator validator = new TransakcijaValidator();

        public IActionResult Index()
        {
            IEnumerable<Transakcija> transakcije = _context.Transakcija
            .Include(t => t.VrstaTransakcije)
            .ToList();

            return View(transakcije);
        }

        // public IActionResult Details(int? id)
        // {
        //     if (id == null)
        //     {
        //         return NotFound();
        //     }

        //     var Transakcija = _context.Transakcija
        //         .Include(z => z.Zahtjev)
        //         .FirstOrDefault(m => m.TransakcijaId == id);

        //     if (Transakcija == null)
        //     {
        //         return NotFound();
        //     }

        //     return View(Transakcija);
        // }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareTransakcijaProps();
            Transakcija Transakcija = new Transakcija();
            var viewModel = new TransakcijaViewModel
            {
                Transakcija = Transakcija
            };
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int TransakcijaId)
        {
            await PrepareTransakcijaProps();
            Transakcija Transakcija = new Transakcija();
            Transakcija = _context.Transakcija.Find(TransakcijaId);
            var viewModel = new TransakcijaViewModel
            {
                Transakcija = Transakcija
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TransakcijaViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                Console.WriteLine("usao u create");
                _context.Transakcija.Add(viewModel.Transakcija);

                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TransakcijaViewModel viewModel, int TransakcijaId)
        {
            if (ModelState.IsValid)
            {
                var Transakcija = _context.Transakcija.Find(TransakcijaId);
                Transakcija.IbanIsporučitelja = viewModel.Transakcija.IbanIsporučitelja;
                Transakcija.IbanPrimatelja = viewModel.Transakcija.IbanPrimatelja;
                Transakcija.Iznos = viewModel.Transakcija.Iznos;
                Transakcija.ProjektnaKarticaIsporučiteljId = viewModel.Transakcija.ProjektnaKarticaIsporučiteljId;
                Transakcija.ProjektnaKarticaPrimateljId = viewModel.Transakcija.ProjektnaKarticaPrimateljId;

                _context.Transakcija.Update(Transakcija);

                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int TransakcijaId)
        {
            var TransakcijaToBeDeleted = _context.Transakcija.Find(TransakcijaId);
            if (TransakcijaToBeDeleted != null)
            {
                try
                {
                    _context.Remove(TransakcijaToBeDeleted);
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: {0}", ex.GetBaseException().Message);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task PrepareTransakcijaProps()
        {
            await PrepareProjektnaKarticaDropdownList();
            await PrepareVrstaTransakcijeDropdownList();
        }


        private async Task PrepareProjektnaKarticaDropdownList()
        {
            var ProjektneKartice = await _context.ProjektnaKartica
                            .OrderBy(t => t.ProjektnaKarticaId)
                            .Select(d => new SelectListItem
                            {
                                Value = d.ProjektnaKarticaId.ToString(),
                                Text = d.ProjektnaKarticaId.ToString()
                            }).ToListAsync();

            ViewBag.ProjektneKartice = new SelectList(ProjektneKartice, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
        }


        private async Task PrepareVrstaTransakcijeDropdownList()
        {
            var VrsteTransakcije = await _context.VrstaTransakcije
                            .OrderBy(t => t.VrstaTransakcijeId)
                            .Select(d => new SelectListItem
                            {
                                Value = d.VrstaTransakcijeId.ToString(),
                                Text = d.Ime
                            }).ToListAsync();

            ViewBag.VrsteTransakcije = new SelectList(VrsteTransakcije, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
        }

        //2. predaja -------------------------------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUlaz(int ProjektnaKarticaId, int Iznos, int IbanPrimatelja, int IbanIsporučitelja, int VrstaId, int? ProjektnaKarticaIsporučiteljId, string VrstaIme)
        {
            var vrstaId = _context.VrstaTransakcije.FirstOrDefault(e => e.Ime == VrstaIme).VrstaTransakcijeId;

            var newTransakcija = new Transakcija
            {
                IbanIsporučitelja = IbanIsporučitelja,
                IbanPrimatelja = IbanPrimatelja,
                Iznos = Iznos,
                VrstaTransakcijeId = vrstaId,
                ProjektnaKarticaIsporučiteljId = ProjektnaKarticaIsporučiteljId,
                ProjektnaKarticaPrimateljId = ProjektnaKarticaId
            };

            var result = validator.Validate(newTransakcija);
            try
            {
                if (result.IsValid)
                {

                    _context.Transakcija.Add(newTransakcija);
                    _context.SaveChanges();

                    TempData[Constants.ErrorOccurred] = false;
                    TempData["SuccessMessage"] = $"Transakcija dodana.";
                    return RedirectToAction("Edit2", "ProjektnaKartica", new { ProjektnaKarticaId = ProjektnaKarticaId });
                }
                else
                {
                    TempData["errorMessage"] = $"Transakciju nije moguće dodati.";
                    return RedirectToAction("Edit2", "ProjektnaKartica", new { ProjektnaKarticaId = ProjektnaKarticaId });
                }
            } catch (Exception ex)
            {
                TempData["errorMessage"] = $"Transakciju nije moguće dodati.";
                return RedirectToAction("Edit2", "ProjektnaKartica", new { ProjektnaKarticaId = ProjektnaKarticaId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateIzlaz(int ProjektnaKarticaId, int Iznos, int IbanPrimatelja, int IbanIsporučitelja, int VrstaId, int? ProjektnaKarticaPrimateljId, string VrstaIme)
        {
            var vrstaId = _context.VrstaTransakcije.FirstOrDefault(e => e.Ime == VrstaIme).VrstaTransakcijeId;

            var newTransakcija = new Transakcija
            {
                IbanIsporučitelja = IbanIsporučitelja,
                IbanPrimatelja = IbanPrimatelja,
                Iznos = Iznos,
                VrstaTransakcijeId = vrstaId,
                ProjektnaKarticaIsporučiteljId = ProjektnaKarticaId,
                ProjektnaKarticaPrimateljId = ProjektnaKarticaPrimateljId
            };

            var result = validator.Validate(newTransakcija);
            try
            {
                if (result.IsValid)
                {

                    _context.Transakcija.Add(newTransakcija);
                    _context.SaveChanges();

                    TempData[Constants.ErrorOccurred] = false;
                    TempData["SuccessMessage"] = $"Transakcija dodana.";
                    return RedirectToAction("Edit2", "ProjektnaKartica", new { ProjektnaKarticaId = ProjektnaKarticaId });
                }
                else
                {
                    TempData["errorMessage"] = $"Transakciju nije moguće dodati.";
                    Console.WriteLine("Exception: {0}", result.Errors);
                    return RedirectToAction("Edit2", "ProjektnaKartica", new { ProjektnaKarticaId = ProjektnaKarticaId });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex.GetBaseException().Message);
                TempData["errorMessage"] = $"Transakciju nije moguće dodati.";
                return RedirectToAction("Edit2", "ProjektnaKartica", new { ProjektnaKarticaId = ProjektnaKarticaId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete2(int TransakcijaId, int ProjektnaKarticaId)
        {
            var transakcijaToBeDeleted = _context.Transakcija.Find(TransakcijaId);
            if (transakcijaToBeDeleted != null)
            {
                try
                {
                    _context.Remove(transakcijaToBeDeleted);
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = $"Transakcija {transakcijaToBeDeleted.TransakcijaId} uspješno obrisana.";
                }
                catch (Exception ex)
                {
                    TempData["errorMessage"] = $"Transakcija {transakcijaToBeDeleted.TransakcijaId} se ne može obrisati.";
                }
            }
            else
            {
                TempData["errorMessage"] = $"Transakcija {transakcijaToBeDeleted.TransakcijaId} se ne može obrisati.";
            }

            return RedirectToAction("Edit2", "ProjektnaKartica", new { ProjektnaKarticaId = ProjektnaKarticaId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUlaz(ProjektnaKarticaTransakcijaViewModel viewModel, int TransakcijaId, int ProjektnaKarticaPrimateljId)
        {

            var newTransakcija = viewModel.UlazneTransakcije.FirstOrDefault(e => e.TransakcijaId == TransakcijaId);

            var vrsta = _context.VrstaTransakcije.FirstOrDefault(e => e.Ime == newTransakcija.VrstaTransakcije.Ime);
            newTransakcija.VrstaTransakcije = vrsta;

            var projektnaKartica = _context.ProjektnaKartica.Find(ProjektnaKarticaPrimateljId);
            newTransakcija.ProjektnaKarticaPrimateljId = ProjektnaKarticaPrimateljId;
            newTransakcija.ProjektnaKarticaPrimatelj = projektnaKartica;

            try
            {
                _context.Transakcija.Update(newTransakcija);
                _context.SaveChanges();

                TempData[Constants.ErrorOccurred] = false;
                TempData["SuccessMessage"] = $"Izmjene nad transakcijom {TransakcijaId} su spremljene.";
            
            } catch (Exception ex) 
            {
                TempData[Constants.ErrorOccurred] = true;
                TempData.Remove(Constants.ErrorOccurred);
                TempData["ErrorMessage"] = $"Izmjene nad transakcijom {TransakcijaId} nije moguće spremiti.";
            }

            return RedirectToAction("Edit2", "ProjektnaKartica", new
            {
                ProjektnaKarticaId = ProjektnaKarticaPrimateljId
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditIzlaz(ProjektnaKarticaTransakcijaViewModel viewModel, int TransakcijaId, int ProjektnaKarticaIsporučiteljId)
        {

            var newTransakcija = viewModel.IzlazneTransakcije.FirstOrDefault(e => e.TransakcijaId == TransakcijaId);

            var vrsta = _context.VrstaTransakcije.FirstOrDefault(e => e.Ime == newTransakcija.VrstaTransakcije.Ime);
            newTransakcija.VrstaTransakcije = vrsta;

            var projektnaKartica = _context.ProjektnaKartica.Find(ProjektnaKarticaIsporučiteljId);
            newTransakcija.ProjektnaKarticaIsporučiteljId = ProjektnaKarticaIsporučiteljId;
            newTransakcija.ProjektnaKarticaIsporučitelj = projektnaKartica;

            try
            {
                _context.Transakcija.Update(newTransakcija);
                _context.SaveChanges();

                TempData[Constants.ErrorOccurred] = false;
                TempData["SuccessMessage"] = $"Izmjene nad transakcijom {TransakcijaId} su spremljene.";

            }
            catch (Exception ex)
            {
                TempData[Constants.ErrorOccurred] = true;
                TempData.Remove(Constants.ErrorOccurred);
                TempData["ErrorMessage"] = $"Izmjene nad transakcijom {TransakcijaId} nije moguće spremiti.";
            }

            return RedirectToAction("Edit2", "ProjektnaKartica", new
            {
                ProjektnaKarticaId = ProjektnaKarticaIsporučiteljId
            });
        }
    }   
}
