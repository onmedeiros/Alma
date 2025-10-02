using Alma.Flows.Core.CustomActivities.Models;
using FluentValidation;

namespace Alma.Modules.Flows.Validators.CustomActivities
{
    public class CustomActivityPortEditValidator : AbstractValidator<CustomActivityPortEditModel>
    {
        public CustomActivityPortEditValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("O nome da porta é obrigatório.");
        }
    }
}