using RPPP_WebApp.Models;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels
{
    public class SuradnikDetailView
    {
        [Display(Name = "Email:")]
        [Required(ErrorMessage = "Email je obavezno polje")]
        public string Email { get; set; }

        [Display(Name = "Ime:")]
        [Required(ErrorMessage = "Ime je obavezno polje")]
        public string Ime { get; set; }

        [Display(Name = "Prezime:")]
        [Required(ErrorMessage = "Prezime je obavezno polje")]
        public string Prezime { get; set; }

        [Display(Name = "Mjesto Stanovanja:")]
        [Required(ErrorMessage = "Mjesto stanovanja je obavezno polje")]
        public string MjestoStanovanja { get; set; }

        [Display(Name = "Telefon:")]
        [Required(ErrorMessage = "Telefon je obavezno polje")]
        public string BrojTelefona { get; set; }

        [Display(Name = "URL:")]
        public string URL { get; set; }

        [Display(Name = "Nadređeni:")]
        public string NadredeniEmail { get; set; }

        public Suradnik nadredeniSuradnik { get; set; }

        public IEnumerable<Posao> Posao { get; set; }

        public IEnumerable<Uloga> Uloga { get; set; }

        public SuradnikDetailView()
        {
            this.Posao = new List<Posao>();
            this.Uloga = new List<Uloga>();
        }
    }
}
