using Alma.Modules.Workflows.Models.Instances;
using FluentValidation;

namespace Alma.Modules.Workflows.Validators.InstanceSchedules
{
    public class InstanceScheduleCreateValidator : AbstractValidator<InstanceScheduleCreateModel>
    {
        public InstanceScheduleCreateValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("O nome do agendamento é obrigatório.");
        }
    }
}
