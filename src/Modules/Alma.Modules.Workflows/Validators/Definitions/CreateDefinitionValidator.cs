using Alma.Modules.Workflows.Models.Definitions;
using FluentValidation;

namespace Alma.Modules.Workflows.Validators.Definitions
{
    public class CreateDefinitionValidator : AbstractValidator<CreateDefinitionModel>
    {
        public CreateDefinitionValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("O nome da definição é obrigatório.");
        }
    }
}
