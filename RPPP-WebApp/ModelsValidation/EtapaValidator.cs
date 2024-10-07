using FluentValidation;
using RPPP_WebApp.Models;
using System;
using System.Text.RegularExpressions;

namespace RPPP_WebApp.ModelsValidation
{
    public class EtapaValidator : AbstractValidator<Etapa>
    {
        public EtapaValidator()
        {
            RuleFor(z => z.Ime).NotEmpty().WithMessage("Ime etape je obavezno.");
            RuleFor(z => z.Ime).MaximumLength(20).WithMessage("Ime etape ne smije biti duže od 20 znakova.");

            RuleFor(z => z.Opis).NotEmpty().WithMessage("Opis etape je obavezan.");
            RuleFor(z => z.Opis).MaximumLength(100).WithMessage("Opis etape ne smije biti duži od 100 znakova.");

            RuleFor(z => z.PlanProjektaId).NotEmpty().WithMessage("ID plana projekta kojemu etapa pripada je obavezan.");
            RuleFor(z => z.AktivnostId).NotEmpty().WithMessage("Aktivnost etape je obavezna.");
        }
    }
}
