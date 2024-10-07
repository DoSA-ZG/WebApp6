using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class ZadatakPageView
    {
        public IEnumerable<Zadatak> Zadatak { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
