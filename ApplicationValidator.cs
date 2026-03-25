using FluentValidation;
using GestioneSicurezze.Models;

namespace GestioneSicurezze
{
    public class ApplicationValidator : AbstractValidator<ModelloXray>
    {
        public ApplicationValidator()
        {
            //RuleFor(x => x.NrEntrata).NotEmpty().WithMessage("Il campo Nr. Entrata è obbligatorio.").WithSeverity(Severity.Error);
            RuleFor(x => x.Awb).NotEmpty().WithMessage("AWB obbligatorio").WithSeverity(Severity.Error);
            RuleFor(x => x.Colli).NotEmpty()
                .Must(value => int.TryParse(value, out _))
                .WithMessage("COLLI obbligatorio e numerico").WithSeverity(Severity.Error);
            RuleFor(x => x.Peso).NotEmpty().WithMessage("PESO obbligatorio").WithSeverity(Severity.Error);
            RuleFor(x => x.Destinazione).NotEmpty().WithMessage("DESTINAZIONE obbligatoria").WithSeverity(Severity.Error);
            RuleFor(x => x.Contenuto).NotEmpty().WithMessage("CONTENUTO obbligatorio").WithSeverity(Severity.Error);
            RuleFor(x => x.Cliente).NotEmpty().WithMessage("CLIENTE obbligatorio").WithSeverity(Severity.Error);
            RuleFor(x => x.Operatore).NotEmpty().WithMessage("OPERATORE obbligatorio").WithSeverity(Severity.Error);
            RuleFor(x => x.CodiceEnac).NotEmpty().WithMessage("CODICE ENAC obbligatorio").WithSeverity(Severity.Error);            
        }
    }
}
