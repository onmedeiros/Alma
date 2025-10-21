using Alma.Workflows.Core.CustomActivities.Models;
using FluentValidation;

namespace Alma.Modules.Workflows.Validators.CustomActivities
{
    public class CustomActivityParameterEditValidator : AbstractValidator<CustomActivityParameterEditModel>
    {
        public CustomActivityParameterEditValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("O nome do parâmetro é obrigatório.");
        }
    }
}