using Alma.Blazor.Models;
using FluentValidation;

namespace Alma.Blazor.Validators
{
    public class CreateApplicationValidator : AbstractValidator<CreateApplicationModel>
    {
        public CreateApplicationValidator()
        {
            RuleFor(x => x.OrganizationId)
                .NotEmpty()
                .WithMessage("O ID da organização não foi definido.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("O nome da aplicação é obrigatório.");
        }
    }
}