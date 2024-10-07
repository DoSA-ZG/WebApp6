using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class ZahtjevPageView
    {
        public IEnumerable<Zahtjev> Zahtjevi { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
