using Alma.Modules.Flows.Models.Definitions;
using FluentValidation;

namespace Alma.Modules.Flows.Validators.Definitions
{
    public class DefinitionPublishValidator : AbstractValidator<DefinitionPublishModel>
    {
        public DefinitionPublishValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("O nome da versão é obrigatório.");

            RuleFor(x => x.Definition)
                .NotNull()
                .WithMessage("A definição do fluxo de trabalho é obrigatória.");
        }
    }
}
