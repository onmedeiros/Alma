using Alma.Integrations.Apis.Models;
using FluentValidation;

namespace Alma.Integrations.Apis.Validators
{
    public class ApiCreateValidator : AbstractValidator<ApiCreateModel>
    {
        public ApiCreateValidator()
        {
            RuleFor(x => x.OrganizationId)
                .NotEmpty()
                .WithMessage("Organization ID is required.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("O campo Nome é obrigatório.")
                .MaximumLength(100)
                .WithMessage("O campo Nome não pode exceder 100 caracteres.");

            RuleFor(x => x.Path)
                .NotEmpty()
                .WithMessage("O campo Caminho é obrigatório.")
                .Matches(@"^[a-zA-Z0-9_]+$")
                .WithMessage("O Caminho base não pode conter caracteres especiais. São aceitos apenas letras, numeros e underlines.")
                .MaximumLength(200)
                .WithMessage("O Caminho base não pode exceder 200 caracteres.");

            RuleFor(x => x.IsActive)
                .NotNull()
                .WithMessage("O status é obrigatório.");
        }
    }
}