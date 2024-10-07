using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Data;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers
{

    /// <summary>
    /// Razred koji sadrži metode za dohvaćanje podataka za autocomplete polja.
    /// </summary>
    public class AutoCompleteController : Controller
    {

        private readonly RPPP06Context _context;
        private readonly AppSettings appSettings;


        /// <summary>
        /// Konstruktor razreda AutoCompleteController.
        /// </summary>
        /// <param name="context">Kontekst baze podataka.</param>
        /// <param name="options">Opcije aplikacije.</param>
        public AutoCompleteController(RPPP06Context context, IOptionsSnapshot<AppSettings> options)
        {
            _context = context;
            appSettings = options.Value;
        }

        public async Task<IEnumerable<StringLabel>> nadredeniSuradnik(string term)
        {
            var query = _context.Suradnik
                            .Select(m => new StringLabel
                            {
                                Id = m.Email,
                                Label = m.Ime + " " + m.Prezime
                            })
                            .Where(l => l.Label.Contains(term));

            var list = await query.OrderBy(l => l.Label)
                                  .ThenBy(l => l.Id)
                                  .Take(appSettings.AutoCompleteCount)
                                  .ToListAsync();
            return list;
        }

        public async Task<IEnumerable<IntLabel>> vrstaPosla(string term)
        {
            var query = _context.VrstaPosla
                            .Select(m => new IntLabel
                            {
                                Id = m.VrstaPoslaId,
                                Label = m.Ime
                            })
                            .Where(l => l.Label.Contains(term));

            var list = await query.OrderBy(l => l.Label)
                                  .ThenBy(l => l.Id)
                                  .Take(appSettings.AutoCompleteCount)
                                  .ToListAsync();
            return list;
        }

        public async Task<IEnumerable<IntLabel>> Zadatak(string term)
        {
            var query = _context.Zadatak.Include(p => p.Zahtjev)
                             .ThenInclude(p => p.PlanProjekta)
                                 .ThenInclude(p => p.Projekt)
                            .Select(m => new IntLabel
                            {
                                Id = m.ZadatakId,
                                Label = m.OpisZadatka + " (" + m.Zahtjev.PlanProjekta.Projekt.Ime + ")",
                            })
                            .Where(l => l.Label.Contains(term));

            var list = await query.OrderBy(l => l.Label)
                                  .ThenBy(l => l.Id)
                                  .Take(appSettings.AutoCompleteCount)
                                  .ToListAsync();
            return list;
        }

        public async Task<IEnumerable<IntLabel>> Projekt(string term)
        {
            var query = _context.Projekt
                            .Select(p => new IntLabel
                            {
                                Id = p.ProjektId,
                                Label = p.Kratica + " (" + p.ProjektId + ")"
                            })
                            .Where(l => l.Label.Contains(term));

            var list = await query.OrderBy(l => l.Label)
                                  .ThenBy(l => l.Id)
                                  .Take(appSettings.AutoCompleteCount)
                                  .ToListAsync();
            return list;
        }

        public async Task<IEnumerable<IntLabel>> PlanProjekta(string term)
        {
            var query = _context.PlanProjekta
                            .Select(p => new IntLabel
                            {
                                Id = p.PlanProjektaId,
                                Label = p.Projekt.Ime
                            })
                            .Where(l => l.Label.Contains(term));

            var list = await query.OrderBy(l => l.Label)
                                  .ThenBy(l => l.Id)
                                  .Take(appSettings.AutoCompleteCount)
                                  .ToListAsync();
            return list;
        }

        public async Task<IEnumerable<IntLabel>> Prioritet(string term)
        {
            var query = _context.Prioritet
                            .Select(p => new IntLabel
                            {
                                Id = p.PrioritetId,
                                Label = p.Ime,
                            })
                            .Where(l => l.Label.Contains(term));

            var list = await query.OrderBy(l => l.Label)
                                  .ThenBy(l => l.Id)
                                  .Take(appSettings.AutoCompleteCount)
                                  .ToListAsync();
            return list;
        }

        public async Task<IEnumerable<IntLabel>> TipZahtjeva(string term)
        {
            var query = _context.TipZahtjeva
                            .Select(z => new IntLabel
                            {
                                Id = z.TipZahtjevaId,
                                Label = z.Ime
                            })
                            .Where(l => l.Label.Contains(term));

            var list = await query.OrderBy(l => l.Label)
                                  .ThenBy(l => l.Id)
                                  .Take(appSettings.AutoCompleteCount)
                                  .ToListAsync();
            return list;
        }

        public async Task<IEnumerable<IntLabel>> Status(string term)
        {
            var query = _context.Status
                            .Select(z => new IntLabel
                            {
                                Id = z.StatusId,
                                Label = z.Ime
                            })
                            .Where(l => l.Label.Contains(term));

            var list = await query.OrderBy(l => l.Label)
                                  .ThenBy(l => l.Id)
                                  .Take(appSettings.AutoCompleteCount)
                                  .ToListAsync();
            return list;
        }

        public async Task<IEnumerable<StringLabel>> NadredeniSuradnikEmail(string term)
        {
            var query = _context.Suradnik
                            .Select(m => new StringLabel
                            {
                                Id = m.Email,
                                Label = m.Email
                            })
                            .Where(l => l.Label.Contains(term));

            var list = await query.OrderBy(l => l.Label)
                                  .ThenBy(l => l.Id)
                                  .Take(appSettings.AutoCompleteCount)
                                  .ToListAsync();
            return list;
        }

        public async Task<IEnumerable<IntDoubleLabel>> Etapa(string term)
        {
            var query = _context.Etapa
                            .Select(e => new IntDoubleLabel
                            {
                                Id = e.EtapaId,
                                Label = e.Ime,
                                Opis = e.Opis,
                            })
                            .Where(l => l.Label.Contains(term));

            var list = await query.OrderBy(l => l.Label)
                                  .ThenBy(l => l.Id)
                                  .Take(appSettings.AutoCompleteCount)
                                  .ToListAsync();
            return list;
        }

        public async Task<IEnumerable<IntLabel>> Aktivnost(string term)
        {
            var query = _context.Aktivnost
                            .Select(e => new IntLabel
                            {
                                Id = e.AktivnostId,
                                Label = e.Ime
                            })
                            .Where(l => l.Label.Contains(term));

            var list = await query.OrderBy(l => l.Label)
                                  .ThenBy(l => l.Id)
                                  .Take(appSettings.AutoCompleteCount)
                                  .ToListAsync();
            return list;
        }

        public async Task<IEnumerable<StringLabel>> Voditelj(string term)
        {
            var query = _context.Suradnik
                            .Select(m => new StringLabel
                            {
                                Id = m.Email,
                                Label = m.Email
                            })
                            .Where(l => l.Label.Contains(term));

            var list = await query.OrderBy(l => l.Label)
                                  .ThenBy(l => l.Id)
                                  .Take(appSettings.AutoCompleteCount)
                                  .ToListAsync();
            return list;
        }


        /// <summary>
        /// Metoda koja vraća listu vrsti transakcija koje odgovaraju zadanoj riječi.
        /// </summary>
        /// <param name="term">String koji se traži u imenu vrste transakcije.</param>
        /// <returns>Lista vrsti transakcija koje odgovaraju zadanoj riječi.</returns>
        public async Task<IEnumerable<IntLabel>> VrstaTransakcije(string term)
        {
            var query = _context.VrstaTransakcije
                            .Select(m => new IntLabel
                            {
                                Id = m.VrstaTransakcijeId,
                                Label = m.Ime
                            })
                            .Where(l => l.Label.Contains(term));

            var list = await query.OrderBy(l => l.Label)
                                  .ThenBy(l => l.Id)
                                  .ToListAsync();
            return list;
        }

        /// <summary>
        /// Metoda koja vraća listu projektnih kartica koje odgovaraju zadanoj riječi.
        /// </summary>
        /// <param name="term">Riječ koja se traži u imenu projekt projektne kartice.</param>
        /// <returns>Lista projektnih kartica koje odgovaraju zadanoj riječi.</returns>
        public async Task<IEnumerable<IntLabel>> ProjektnaKartica(string term)
        {
            var query = _context.ProjektnaKartica
                            .Select(m => new IntLabel
                            {
                                Id = m.ProjektnaKarticaId,
                                Label = m.ProjektnaKarticaId.ToString() + " (" + m.Projekt.Ime + ")"
                            })
                            .Where(l => l.Label.Contains(term));

            var list = await query.OrderBy(l => l.Label)
                                  .ThenBy(l => l.Id)
                                  .ToListAsync();

            return list;
        }
    }
}
