using Alma.Workflows.Core.InstanceEndpoints.Models;
using FluentValidation;

namespace Alma.Modules.Workflows.Validators.InstanceEndpoints
{
    public class InstanceEndpointCreateValidator : AbstractValidator<InstanceEndpointCreateModel>
    {
        public InstanceEndpointCreateValidator()
        {
            RuleFor(model => model.Name)
                .NotEmpty()
                .WithMessage("O nome é obrigatório.");

            RuleFor(model => model.Path)
                .NotEmpty()
                .WithMessage("O caminho é obrigatório.");
        }
    }
}