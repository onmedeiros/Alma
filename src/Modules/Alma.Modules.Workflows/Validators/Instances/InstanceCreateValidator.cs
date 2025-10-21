using Alma.Modules.Workflows.Models.Instances;
using FluentValidation;

namespace Alma.Modules.Workflows.Validators.Instances
{
    public class InstanceCreateValidator : AbstractValidator<CreateInstanceModel>
    {
        public InstanceCreateValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("O nome da instância é obrigatório.");
        }
    }
}
