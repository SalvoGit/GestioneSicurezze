using FluentValidation;
using GestioneSicurezze.Models;

namespace GestioneSicurezze
{
    public class ValidazioneSigilli : AbstractValidator<Sigillo>
    {
        public ValidazioneSigilli()
        {            
            RuleFor(x => x.NRSIGILLO).NotEmpty().WithMessage("Il campo NR. SIGILLO è obbligatorio.").WithSeverity(Severity.Error);
            RuleFor(x => x.DESTINAZIONE).NotEmpty().WithMessage("Il campo DESTINAZIONE è obbligatorio.").WithSeverity(Severity.Error);
            //RuleFor(x => x.TARGA).NotEmpty().WithMessage("Il campo TARGA è obbligatorio.").WithSeverity(Severity.Error);
            //RuleFor(x => x.TRASPORTATORE).NotEmpty().WithMessage("Il campo TRASPORTATORE è obbligatorio.").WithSeverity(Severity.Error);
        }
    }
}