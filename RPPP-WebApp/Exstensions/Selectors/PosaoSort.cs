using RPPP_WebApp.Models;
using System.Linq.Expressions;

namespace RPPP_WebApp.Exstensions.Selectors
{
    public static class PosaoSort
    {
        public static IQueryable<Posao> ApplySort(this IQueryable<Posao> query, int sort, bool ascending)
        {
            Expression<Func<Posao, object>> orderSelector = sort switch
            {
                1 => d => d.Opis,
                2 => d => d.VrijemePočetkaRada,
                3 => d => d.VrijemeKrajaRada,
                4 => d => d.VrstaPosla.Ime,
                5 => d => d.Zadatak.Zahtjev.PlanProjekta.Projekt.Ime,
                6 => d => d.Zadatak.OpisZadatka,
                7 => d => d.SuradnikEmailNavigation.Ime + " " + d.SuradnikEmailNavigation.Prezime,
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
