using Alma.Workflows.Monitoring.MonitoringObjectSchemas.Models;
using FluentValidation;

namespace Alma.Workflows.Monitoring.MonitoringObjectSchemas.Validators
{
    public class MonitoringObjectSchemaEditValidator : AbstractValidator<MonitoringObjectSchemaEditModel>
    {
        public MonitoringObjectSchemaEditValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("ID is required.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required.")
                .MaximumLength(100)
                .WithMessage("Name cannot exceed 100 characters.")
                .When(x => x.Name is not null);
        }
    }
}