using RPPP_WebApp.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPPP_WebApp.ModelsPartial
{
    public class ViewSuradnikInfo
    {
        public Suradnik suradnik { get; set; }

        [NotMapped]
        public int Position { get; set; }
    }
}
