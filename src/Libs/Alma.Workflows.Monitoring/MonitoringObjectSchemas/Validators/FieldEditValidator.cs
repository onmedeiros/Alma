using Alma.Workflows.Monitoring.MonitoringObjectSchemas.Models;
using FluentValidation;

namespace Alma.Workflows.Monitoring.MonitoringObjectSchemas.Validators
{
    public class FieldEditValidator : AbstractValidator<FieldEditModel>
    {
        public FieldEditValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Field name is required.")
                .MaximumLength(100)
                .WithMessage("Field name cannot exceed 100 characters.");

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Invalid field type.");
        }
    }
}