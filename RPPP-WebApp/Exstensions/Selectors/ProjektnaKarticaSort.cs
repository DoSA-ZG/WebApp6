using RPPP_WebApp.Models;
using System.Linq.Expressions;

namespace RPPP_WebApp.Exstensions.Selectors
{
    /// <summary>
    /// Razred za sortiranje projektnih kartica
    /// </summary>
    public static class ProjektnaKarticaSort
    {

        /// <summary>
        /// Metoda za sortiranje projektnih kartica, ovisno o odabranom stupcu i smjeru sortiranja, vraća sortiranu listu projektnih kartica
        /// </summary>
        /// <param name="query">IQueryable lista projektnih kartica</param>
        /// <param name="sort">Parametar za odabir stupca po kojem se sortira</param>
        /// <param name="ascending">Varijabla koja određuje smjer sortiranja</param>
        /// <returns></returns>
        public static IQueryable<ProjektnaKartica> ApplySort(this IQueryable<ProjektnaKartica> query, int sort, bool ascending)
        {
            Expression<Func<ProjektnaKartica, object>> orderSelector = sort switch
            {
                1 => d => d.ProjektnaKarticaId,
                2 => d => d.Banka,
                3 => d => d.Iban,
                4 => d => d.Stanje,
                5 => d => d.Projekt.Ime,
                6 => d => d.TransakcijaProjektnaKarticaIsporučitelj.OrderBy(t => t.TransakcijaId).FirstOrDefault().TransakcijaId,
                7 => d => d.TransakcijaProjektnaKarticaPrimatelj.OrderBy(t => t.TransakcijaId).FirstOrDefault().TransakcijaId,
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
