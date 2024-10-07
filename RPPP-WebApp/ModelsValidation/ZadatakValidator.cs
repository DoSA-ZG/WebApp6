using FluentValidation;
using RPPP_WebApp.Models;
using System;
using System.Text.RegularExpressions;

namespace RPPP_WebApp.ModelsValidation
{
    public class ZadatakValidator : AbstractValidator<Zadatak>
    {
        public ZadatakValidator()
        {
            RuleFor(z => z.OpisZadatka).NotEmpty().WithMessage("Opis zadatka je obavezan.");
            RuleFor(z => z.PlaniraniPočetak).NotEmpty().WithMessage("Planirani početak je obavezan.");
            RuleFor(z => z.PlaniraniKraj).NotEmpty().WithMessage("Planirani kraj je obavezan.");
            RuleFor(z => z.PlaniraniKraj).GreaterThan(z => z.PlaniraniPočetak).WithMessage("Planirani kraj mora biti nakon planiranog početka.");
            RuleFor(z => z.StvarniKraj).GreaterThan(z => z.StvarniPočetak).When(z => z.StvarniPočetak.HasValue && z.StvarniKraj.HasValue).WithMessage("Stvarni kraj mora biti nakon stvarnog početka.");
            RuleFor(z => z.PlaniraniPočetak).Must(date => date >= DateTime.Today).WithMessage("Planirani početak mora biti nakon današnjeg dana.");
            RuleFor(z => z.PlaniraniKraj).Must(date => date >= DateTime.Today).WithMessage("Planirani kraj mora biti nakon današnjeg dana.");
            RuleFor(z => z.StvarniPočetak).Must(date => !date.HasValue || date.Value >= DateTime.Today).WithMessage("Stvarni početak mora biti nakon današnjeg dana.");
            RuleFor(z => z.StvarniKraj).Must(date => !date.HasValue || date.Value >= DateTime.Today).WithMessage("Stvarni kraj mora biti nakon današnjeg dana.");
            RuleFor(z => z.ZahtjevId).NotEmpty().WithMessage("ID zahtjeva je obavezan.");
            RuleFor(z => z.StatusId).NotEmpty().WithMessage("ID statusa je obavezan.");
            RuleFor(z => z.NositeljEmail).NotEmpty().WithMessage("Email nositelja je obavezan.");
            RuleFor(z => z.NositeljEmail).EmailAddress().WithMessage("Neispravna adresa e-pošte.");
        }
    }
}
