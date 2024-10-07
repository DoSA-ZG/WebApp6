using RPPP_WebApp.Models;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels
{
    /// <summary>
    /// Razred koji omogućava prikaz projektnih kartica na stranici
    /// </summary>
    public class ProjektnaKarticaPageView
    {

        public IEnumerable<ProjektnaKartica> ProjektneKartice { get; set; }
        public PagingInfo PagingInfo { get; set; }

        //Test

    }
}
