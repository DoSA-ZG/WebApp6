using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class PlanProjektaViewModel
    {
        public PlanProjekta PlanProjekta { get; set; }

        public IEnumerable<Etapa> Etape { get; set; }
    }
}
