using Alma.Modules.Workflows.Models.CustomActivities;
using FluentValidation;

namespace Alma.Modules.Workflows.Validators.CustomActivities
{
    public class CustomActivityCreateValidator : AbstractValidator<CustomActivityCreateModel>
    {
        public CustomActivityCreateValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("O nome da atividade personalizada é obrigatório.");
        }
    }
}