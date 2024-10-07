using FluentValidation;
using RPPP_WebApp.Models;
using System.Text.RegularExpressions;
namespace RPPP_WebApp.ModelsValidation
{
    public class SuradnikValidator : AbstractValidator<Suradnik>
    {
        public SuradnikValidator() {
            RuleFor(d => d.Email)
        .NotEmpty().WithMessage("Email je obavezno polje")
        .Matches(@"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$")
        .WithMessage("Unesite ispravan email format");

        RuleFor(d => d.Ime).NotEmpty().WithMessage("Ime je obavezno polje");

        RuleFor(d => d.Prezime).NotEmpty().WithMessage("Prezime je obavezno polje");

        RuleFor(d => d.MjestoStanovanja).NotEmpty().WithMessage("Mjesto stanovanja je obavezno polje");



        }
    }
}
