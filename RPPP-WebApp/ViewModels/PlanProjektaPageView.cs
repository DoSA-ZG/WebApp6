using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class PlanProjektaPageView
    {
        public IEnumerable<PlanProjekta> Planovi { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
