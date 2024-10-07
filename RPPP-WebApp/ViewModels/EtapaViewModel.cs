using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class EtapaViewModel
    {
        public Etapa Etapa { get; set; }

        public Aktivnost Aktivnosti { get; set; }

        public EtapaViewModel()
        {
            this.Aktivnosti = new Aktivnost();
        }
    }
}
