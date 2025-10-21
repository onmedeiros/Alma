using Alma.Workflows.Core.InstanceEndpoints.Models;
using FluentValidation;

namespace Alma.Modules.Workflows.Validators.InstanceEndpoints
{
    public class InstanceEndpointEditValidator : AbstractValidator<InstanceEndpointEditModel>
    {
        public InstanceEndpointEditValidator()
        {
            RuleFor(model => model.Name)
                .NotEmpty()
                .WithMessage("O nome é obrigatório.");

            RuleFor(model => model.Path)
                .NotEmpty()
                .WithMessage("O caminho é obrigatório.");

            RuleFor(model => model.Method)
                .IsInEnum()
                .WithMessage("O método é obrigatório.");
        }
    }
}