using RPPP_WebApp.Models;
using System.Linq.Expressions;

namespace RPPP_WebApp.Exstensions.Selectors
{
    public static class EtapaSort
    {
        public static IQueryable<Etapa> ApplySort(this IQueryable<Etapa> query, int sort, bool ascending)
        {
            Expression<Func<Etapa, object>> orderSelector = sort switch
            {
                1 => d => d.EtapaId,
                2 => d => d.Ime,
                3 => d => d.Opis,
                4 => d => d.PlanProjektaId,
                5 => d => d.Aktivnost.Ime,
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
