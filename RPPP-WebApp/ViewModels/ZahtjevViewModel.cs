using RPPP_WebApp.Models;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels
{
    public class ZahtjevViewModel
    {
        public Zahtjev Zahtjev { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
