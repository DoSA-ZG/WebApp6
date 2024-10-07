using FluentValidation;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ModelsValidation
{
    public class PlanProjektaValidator : AbstractValidator<PlanProjekta>
    {
        public PlanProjektaValidator()
        {
            RuleFor(z => z.PlaniraniPočetak).NotEmpty().WithMessage("Planirani početak je obavezan.");
            RuleFor(z => z.PlaniraniPočetak).Must(date => date.Date >= DateTime.Today).WithMessage("Planirani početak mora biti nakon današnjeg dana.");

            RuleFor(z => z.PlaniraniKraj).NotEmpty().WithMessage("Planirani kraj je obavezan.");
            RuleFor(z => z.PlaniraniKraj).GreaterThan(z => z.PlaniraniPočetak).WithMessage("Planirani kraj mora biti nakon planiranog početka.");
            RuleFor(z => z.PlaniraniKraj).Must(date => date.Date >= DateTime.Today).WithMessage("Planirani kraj mora biti nakon današnjeg dana.");

            RuleFor(z => z.StvarniPočetak).Must(date => !date.HasValue || date.Value >= DateTime.Today).WithMessage("Stvarni početak mora biti nakon današnjeg dana.");

            RuleFor(z => z.StvarniKraj).Must(date => !date.HasValue || date.Value >= DateTime.Today).WithMessage("Stvarni kraj mora biti nakon današnjeg dana.");
            RuleFor(z => z.StvarniKraj).GreaterThan(z => z.StvarniPočetak).When(z => z.StvarniPočetak.HasValue && z.StvarniKraj.HasValue).WithMessage("Stvarni kraj mora biti nakon stvarnog početka.");

            RuleFor(z => z.ProjektId).NotEmpty().WithMessage("ID projekta je obavezan.");

            RuleFor(z => z.VoditeljEmail).NotEmpty().WithMessage("Voditeljev email je obavezan.");
            RuleFor(z => z.VoditeljEmail).EmailAddress().WithMessage("Voditeljev email mora biti u ispravnom formatu.");
        }
    }
}
