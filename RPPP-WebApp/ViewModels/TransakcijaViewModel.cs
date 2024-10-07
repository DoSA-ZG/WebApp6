using RPPP_WebApp.Models;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels
{
    /// <summary>
    /// Razred koji omogućava prikaz detalja transakcije.
    /// </summary>
    public class TransakcijaViewModel
    {
        public Transakcija Transakcija { get; set; }

        public virtual ProjektnaKartica ProjektnaKarticaIsporučitelj { get; set; }

        public virtual ProjektnaKartica ProjektnaKarticaPrimatelj { get; set; }

        [Required(ErrorMessage = "Vrsta transakcije is required.")]
        public virtual VrstaTransakcije VrstaTransakcije { get; set; }
    }
}
