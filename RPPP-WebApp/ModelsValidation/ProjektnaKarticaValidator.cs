using FluentValidation;
using RPPP_WebApp.Models;
using System.Text.RegularExpressions;

namespace RPPP_WebApp.ModelsValidation
{
    /// <summary>
    /// Razred koji služi za validaciju modela ProjektnaKartica.
    /// </summary>
    public class ProjektnaKarticaValidator : AbstractValidator<ProjektnaKartica>
    {
        /// <summary>
        /// Konstruktor koji služi za validaciju modela ProjektnaKartica. Određuje uvjete koje mora zadovoljiti svaki objekt tipa ProjektnaKartica.
        /// </summary>
        public ProjektnaKarticaValidator()
        {
            RuleFor(z => z.Banka).NotEmpty().WithMessage("Banka je obavezna.");

            RuleFor(z => z.Iban).NotEmpty().WithMessage("Iban je obavezan.")
                .Must(iban => iban.ToString().Length == 8).WithMessage("Iban mora imati točno 8 znamenki.");


            RuleFor(z => z.Stanje).NotEmpty().WithMessage("Stanje je obavezno.");

            RuleFor(z => z.ProjektId).NotEmpty().WithMessage("ID projekta je obavezan.");
        }
    }
}
