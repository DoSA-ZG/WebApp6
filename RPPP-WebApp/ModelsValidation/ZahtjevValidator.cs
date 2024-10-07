using FluentValidation;
using RPPP_WebApp.Models;
using System.Text.RegularExpressions;

namespace RPPP_WebApp.ModelsValidation
{
    public class ZahtjevValidator : AbstractValidator<Zahtjev>
    {
        public ZahtjevValidator()
        {
            RuleFor(z => z.Ime).NotEmpty().WithMessage("Ime je obavezno.");
            RuleFor(z => z.Ime).Matches("^[A-Za-z ]+$").WithMessage("Ime može sadržavati samo slova.");

            RuleFor(z => z.Opis).NotEmpty().WithMessage("Opis je obavezan.");

            RuleFor(z => z.PlanProjektaId).NotEmpty().WithMessage("ID plana projekta je obavezan.");

            RuleFor(z => z.PrioritetId).NotEmpty().WithMessage("ID prioriteta je obavezan.");

            RuleFor(z => z.TipZahtjevaId).NotEmpty().WithMessage("ID tipa zahtjeva je obavezan.");
        }
    }
}
