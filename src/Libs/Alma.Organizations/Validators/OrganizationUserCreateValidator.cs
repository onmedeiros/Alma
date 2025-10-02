using Alma.Organizations.Models;
using FluentValidation;

namespace Alma.Organizations.Validators
{
    public class OrganizationUserCreateValidator : AbstractValidator<OrganizationUserCreateModel>
    {
        public OrganizationUserCreateValidator()
        {
            RuleFor(x => x.OrganizationId)
                .NotEmpty()
                .WithMessage("Organization ID is required.");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required.");
        }
    }
}