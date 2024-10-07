using RPPP_WebApp.Models;
using System.Linq.Expressions;

namespace RPPP_WebApp.Exstensions.Selectors
{
    public static class PlanProjektaSort
    {
        public static IQueryable<PlanProjekta> ApplySort(this IQueryable<PlanProjekta> query, int sort, bool ascending)
        {
            Expression<Func<PlanProjekta, object>> orderSelector = sort switch
            {
                1 => d => d.PlanProjektaId,
                2 => d => d.PlaniraniPočetak,
                3 => d => d.PlaniraniKraj,
                4 => d => d.StvarniPočetak,
                5 => d => d.StvarniKraj,
                6 => d => d.Projekt.Kratica,
                7 => d => d.VoditeljEmail,

                8 => d => d.Zahtjev.OrderBy(u => u.Ime).FirstOrDefault().Ime,
                9 => d => d.Etapa.OrderBy(u => u.Opis).FirstOrDefault().Opis,
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
