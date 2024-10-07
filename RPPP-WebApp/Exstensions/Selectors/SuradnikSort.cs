using RPPP_WebApp.Models;
using System.Linq.Expressions;

namespace RPPP_WebApp.Exstensions.Selectors
{
    public static class SuradnikSort
    {
        public static IQueryable<Suradnik> ApplySort(this IQueryable<Suradnik> query, int sort, bool ascending)
        {
            Expression<Func<Suradnik, object>> orderSelector = sort switch
            {
                1 => d => d.Email,
                2 => d => d.Ime,
                3 => d => d.Prezime,
                4 => d => d.MjestoStanovanja,
                5 => d => d.NadređeniEmail,
                6 => d => d.Uloga.OrderBy(u => u.Ime).FirstOrDefault().Ime,
                7 => d => d.BrojTelefona,
                8 => d => d.URL,
                9 => d => d.Posao.OrderBy(u => u.Opis).FirstOrDefault().Opis,
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
