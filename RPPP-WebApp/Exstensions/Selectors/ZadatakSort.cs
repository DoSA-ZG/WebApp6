using RPPP_WebApp.Models;
using System.Linq.Expressions;

namespace RPPP_WebApp.Exstensions.Selectors
{
    public static class ZadatakSort
    {
        public static IQueryable<Zadatak> ApplySort(this IQueryable<Zadatak> query, int sort, bool ascending)
        {
            Expression<Func<Zadatak, object>> orderSelector = sort switch
            {
                1 => z => z.ZadatakId,
                2 => z => z.OpisZadatka,
                3 => z => z.PlaniraniPočetak,
                4 => z => z.PlaniraniKraj,
                5 => z => z.StvarniPočetak,
                6 => z => z.StvarniKraj,
                7 => z => z.Zahtjev.Ime,
                8 => z => z.Status.Ime,
                9 => z => z.NositeljEmail,
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
