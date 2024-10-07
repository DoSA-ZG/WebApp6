using RPPP_WebApp.Models;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels
{
    /// <summary>
    /// Razred koji omogućava prikaz detalja projektne kartice
    /// </summary>
    public class ProjektnaKarticaViewModel
    {
        public ProjektnaKartica ProjektnaKartica { get; set; }

        [Required(ErrorMessage = "Projekt is required.")]
        public Projekt Projekt { get; set; }

        //Test
        
    }
}
