using RPPP_WebApp.Models;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels
{
    public class ZadatakViewModel
    {
        public Zadatak Zadatak { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
