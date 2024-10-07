using Microsoft.AspNetCore.Mvc;
using RPPP_WebApp.Models.JTable;
using RPPP_WebApp.ViewModels;
/*
namespace RPPP_WebApp.Controllers.JTable
{
    [Route("jtable/etapa/[action]")]
    public class EtapaJTableController : JTableController<EtapaController, int, EtapaViewModel>
    {
        public EtapaJTableController(EtapaController controller) : base(controller)
        {

        }

        [HttpPost]
        public async Task<JTableAjaxResult> Update([FromForm] EtapaViewModel model)
        {
            return await base.UpdateItem(model.Etapa.EtapaId, model);
        }

        [HttpPost]
        public async Task<JTableAjaxResult> Delete([FromForm] int IdMjesta)
        {
            return await base.DeleteItem(IdMjesta);
        }
    }
}*/