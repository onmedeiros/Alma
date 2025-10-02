using Alma.Flows.Core.InstanceEndpoints.Models;
using FluentValidation;

namespace Alma.Modules.Flows.Validators.InstanceEndpoints
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