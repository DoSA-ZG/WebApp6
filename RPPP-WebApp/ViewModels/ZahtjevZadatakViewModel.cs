using RPPP_WebApp.Models;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels
{
    public class ZahtjevZadatakViewModel
    {
        public Zahtjev Zahtjev { get; set; }
        public Zadatak Zadatak { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
