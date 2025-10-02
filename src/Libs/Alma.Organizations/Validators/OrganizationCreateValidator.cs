using Alma.Organizations.Models;
using FluentValidation;

namespace Alma.Organizations.Validators
{
    public class OrganizationCreateValidator : AbstractValidator<OrganizationCreateModel>
    {
        public OrganizationCreateValidator()
        {
            RuleFor(x => x.Subdomain)
                .NotEmpty()
                .WithMessage("Subdomain is required.")
                .Matches(@"^[a-z0-9]+(-[a-z0-9]+)*$")
                .WithMessage("Subdomain must be lowercase alphanumeric with optional hyphens.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Organization name is required.")
                .MaximumLength(100)
                .WithMessage("Organization name cannot exceed 100 characters.");
        }
    }
}