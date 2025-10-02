using Alma.Integrations.Apis.Models;
using FluentValidation;

namespace Alma.Integrations.Apis.Validators
{
    public class ApiKeyCreateValidator : AbstractValidator<ApiKeyCreateModel>
    {
        public ApiKeyCreateValidator()
        {
            RuleFor(x => x.OrganizationId)
                .NotEmpty()
                .WithMessage("Organization ID is required.");

            RuleFor(x => x.ApiId)
                .NotEmpty()
                .WithMessage("API ID is required.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("O campo Nome � obrigat�rio.")
                .MaximumLength(100)
                .WithMessage("O campo Nome n�o pode exceder 100 caracteres.");
        }
    }
}
