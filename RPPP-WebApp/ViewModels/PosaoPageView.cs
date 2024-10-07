using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class PosaoPageView
    {

        public IEnumerable<Posao> Posao { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
