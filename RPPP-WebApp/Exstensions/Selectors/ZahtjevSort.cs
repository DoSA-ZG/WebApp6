using RPPP_WebApp.Models;
using System.Linq.Expressions;

namespace RPPP_WebApp.Exstensions.Selectors
{
    public static class ZahtjevSort
    {
        public static IQueryable<Zahtjev> ApplySort(this IQueryable<Zahtjev> query, int sort, bool ascending)
        {
            Expression<Func<Zahtjev, object>> orderSelector = sort switch
            {
                1 => z => z.ZahtjevId,
                2 => z => z.Ime,
                3 => z => z.Opis,
                4 => z => z.PlanProjekta.Projekt.Ime,
                5 => z => z.Prioritet.Ime,
                6 => z => z.TipZahtjeva.Ime,
                7 => z => z.Zadatak.First().OpisZadatka,
                _ => null
            };

            if (orderSelector != null)
            {
                query = ascending ?
                       query.OrderBy(orderSelector) :
                       query.OrderByDescending(orderSelector);
            }

            return query;
        }
    }
}
