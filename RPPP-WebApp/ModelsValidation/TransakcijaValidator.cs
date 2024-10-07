using FluentValidation;
using RPPP_WebApp.Models;
using System;
using System.Text.RegularExpressions;

namespace RPPP_WebApp.ModelsValidation
{

    /// <summary>
    /// Razred koji služi za validaciju modela Transakcija. Određuje uvjete koje mora zadovoljiti svaki objekt tipa Transakcija.
    /// </summary>
    public class TransakcijaValidator : AbstractValidator<Transakcija>
    {
        public TransakcijaValidator()
        {
            RuleFor(z => z.IbanIsporučitelja).NotEmpty().WithMessage("Iban isporučitelja je obavezan.")
                .Must(iban => iban.ToString().Length == 8).WithMessage("Iban mora imati točno 8 znamenki.");

            RuleFor(z => z.IbanPrimatelja).NotEmpty().WithMessage("Iban primatelja je obavezan.")
                .Must(iban => iban.ToString().Length == 8).WithMessage("Iban mora imati točno 8 znamenki.");


            RuleFor(z => z.Iznos).NotEmpty().WithMessage("Iznos je obavezan.");

            RuleFor(z => z.VrstaTransakcijeId).NotEmpty().WithMessage("Vrsta transakcije je obavezna.");
        }
    }
}
