using Alma.Modules.Flows.Models.Instances;
using FluentValidation;

namespace Alma.Modules.Flows.Validators.Instances
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
