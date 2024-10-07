using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class EtapaPageView
    {
        public IEnumerable<Etapa> Etapa { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
