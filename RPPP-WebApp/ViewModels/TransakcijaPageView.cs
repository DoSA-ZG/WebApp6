using RPPP_WebApp.Models;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels
{
    /// <summary>
    /// Razred koji omogućava prikaz transakcija na stranici
    /// </summary>
    public class TransakcijaPageView
    {
        public IEnumerable<Transakcija> Transakcija { get; set; }

        public PagingInfo PagingInfo { get; set; }
    }
}
