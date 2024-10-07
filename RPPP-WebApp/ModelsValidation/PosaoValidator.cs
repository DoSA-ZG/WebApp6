using FluentValidation;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ModelsValidation
{
    public class PosaoValidator : AbstractValidator<Posao>
    {
        public PosaoValidator()
        {
            RuleFor(d => d.Opis)
                .NotEmpty().WithMessage("Opis je obavezno polje!");

            RuleFor(d => d.VrijemePočetkaRada)
                .NotEmpty().WithMessage("Vrijeme početka rada je obavezno polje!")
                .Must((posao, vrijemePočetkaRada) => BeValidStartTime(posao.VrijemeKrajaRada, vrijemePočetkaRada))
                .WithMessage("Vrijeme početka rada mora biti prije vremena kraja rada!");

            RuleFor(d => d.VrijemeKrajaRada)
                .NotEmpty().WithMessage("Vrijeme kraja rada je obavezno polje")
                .Must((posao, vrijemeKrajaRada) => BeValidEndTime(posao.VrijemePočetkaRada, vrijemeKrajaRada))
                .WithMessage("Vrijeme kraja rada mora biti nakon vremena početka rada!");
        }

        private bool BeValidStartTime(DateTime? vrijemeKrajaRada, DateTime vrijemePočetkaRada)
        {
            return !vrijemeKrajaRada.HasValue || vrijemePočetkaRada < vrijemeKrajaRada.Value;
        }

        private bool BeValidEndTime(DateTime? vrijemePočetkaRada, DateTime vrijemeKrajaRada)
        {
            return !vrijemePočetkaRada.HasValue || vrijemeKrajaRada > vrijemePočetkaRada.Value;
        }
    }



}
