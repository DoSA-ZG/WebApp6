#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.Models
{
    public partial class Zadatak
    {
        public int ZadatakId { get; set; }

        [Display(Name = "Opis Zadatka")]
        public string OpisZadatka { get; set; }

        [Display(Name = "Planirani Početak")]
        public DateTime PlaniraniPočetak { get; set; }

        [Display(Name = "Planirani Kraj")]
        public DateTime PlaniraniKraj { get; set; }

        [Display(Name = "Stvarni Početak")]
        public DateTime? StvarniPočetak { get; set; }

        [Display(Name = "Stvarni Kraj")]
        public DateTime? StvarniKraj { get; set; }

        [Display(Name = "Zahtjev")]
        public int ZahtjevId { get; set; }

        [Display(Name = "Status")]
        public int StatusId { get; set; }

        [Display(Name = "Nositelj Email")]
        public string NositeljEmail { get; set; }

        public virtual Suradnik NositeljEmailNavigation { get; set; }

        public virtual Status Status { get; set; }

        public virtual ICollection<Posao> Posao { get; set; } = new List<Posao>();

        public virtual Zahtjev Zahtjev { get; set; }
    }
}
