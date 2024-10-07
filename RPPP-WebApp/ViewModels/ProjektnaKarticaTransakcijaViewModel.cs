using RPPP_WebApp.Models;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels
{
    /// <summary>
    /// Razred koji omogućava master-detail prikaz projektnih kartica i transakcija
    /// </summary>
    public class ProjektnaKarticaTransakcijaViewModel
    {
        public ProjektnaKartica ProjektnaKartica { get; set; }

        public IEnumerable<Transakcija> UlazneTransakcije { get; set; }

        public IEnumerable<Transakcija> IzlazneTransakcije { get; set; }
        
    }
}
