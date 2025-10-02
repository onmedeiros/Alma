using Alma.Flows.Monitoring.MonitoringObjectSchemas.Models;
using FluentValidation;

namespace Alma.Flows.Monitoring.MonitoringObjectSchemas.Validators
{
    public class MonitoringObjectSchemaCreateValidator : AbstractValidator<MonitoringObjectSchemaCreateModel>
    {
        public MonitoringObjectSchemaCreateValidator()
        {
            RuleFor(x => x.OrganizationId)
                .NotEmpty()
                .WithMessage("Organization ID is required.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required.")
                .MaximumLength(100)
                .WithMessage("Name cannot exceed 100 characters.");
        }
    }
}