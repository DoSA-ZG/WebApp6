using RPPP_WebApp.Models;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels
{
    public class SuradnikViewModel
    {
        public Suradnik suradnik { get; set; }

        public List<string> SelectedUloge { get; set; }

        [Display(Name = "Odaberite projekte")]
        public List<string> SelectedProjekt { get; set; }
    }
}
