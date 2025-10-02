using Alma.Modules.Flows.Models.Instances;
using FluentValidation;

namespace Alma.Modules.Flows.Validators.InstanceSchedules
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
