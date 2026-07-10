using GestioneSicurezze.Models;
using FluentValidation;

namespace GestioneSicurezze.AutistiTarghe
{
    public class AutistiValidator: AbstractValidator<AutistaTarga>
    {
        public AutistiValidator()
        {
            RuleFor(x => x.Autista).NotEmpty()
                    .MaximumLength(100)
                    .WithMessage("AUTISTA obbligatorio o lunghezza errata").WithSeverity(Severity.Error);
            RuleFor(x => x.Targa).NotEmpty()
                .Length(7)
                .WithMessage("TARGA obbligatorio e di lunghezza 7 caratteri").WithSeverity(Severity.Error);            
        }
    }
}
