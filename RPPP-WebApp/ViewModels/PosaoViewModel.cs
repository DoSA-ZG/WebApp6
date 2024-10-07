using RPPP_WebApp.Models;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels
{
    public class PosaoViewModel
    {
        public Posao posao{ get; set; }

        [Display(Name = "Vrsta posla", Prompt = "Odaberite vrstu posla")]
        [Required(ErrorMessage = "Vrsta posla je obavezno polje!")]
        public string vrstaPosla {  get; set; }

        [Display(Name = "Zadatak")]
        [Required(ErrorMessage = "Zadatak je obavezno polje!")]
        public string zadatak {  get; set; }
    }
}
