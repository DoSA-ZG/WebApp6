using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class SuradnikPageView
    {
        public IEnumerable<Suradnik> Suradnik { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
