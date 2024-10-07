#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.Models
{
    public partial class Zahtjev
    {
        public int ZahtjevId { get; set; }

        [DisplayName("Ime")]
        public string Ime { get; set; }

        [DisplayName("Opis")]
        public string Opis { get; set; }

        [DisplayName("Projekt")]
        public int PlanProjektaId { get; set; }

        [DisplayName("Prioritet")]
        public int PrioritetId { get; set; }

        [DisplayName("Tip zahtjeva")]
        public int TipZahtjevaId { get; set; }

        public virtual PlanProjekta PlanProjekta { get; set; }

        public virtual Prioritet Prioritet { get; set; }

        public virtual TipZahtjeva TipZahtjeva { get; set; }

        public virtual ICollection<Zadatak> Zadatak { get; set; } = new List<Zadatak>();
    }
}
