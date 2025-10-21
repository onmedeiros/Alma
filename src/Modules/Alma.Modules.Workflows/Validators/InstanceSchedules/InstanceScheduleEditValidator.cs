using Alma.Core.Domain.Shared.Extensions;
using Alma.Workflows.Core.InstanceSchedules.Models;
using FluentValidation;

namespace Alma.Modules.Workflows.Validators.InstanceSchedules
{
    public class InstanceScheduleEditValidator : AbstractValidator<InstanceScheduleEditModel>
    {
        public InstanceScheduleEditValidator()
        {
            var minCron = "0 0/5 * * * *";

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("O nome do agendamento é obrigatório.");

            RuleFor(x => x.CronExpression)
                .Must(x => x.IsValidCronExpression())
                .When(x => !string.IsNullOrEmpty(x.CronExpression))
                .WithMessage("A expressão cron informada é inválida.");

            RuleFor(x => x.CronExpression)
                .Must(x => x.IsCronAtLeastEvery(minCron))
                .When(x => !string.IsNullOrEmpty(x.CronExpression))
                .WithMessage(x => $"A expressão cron deve ser de no mínimo {minCron.GetCronDescription()}. Exemplo: {minCron}.");
        }
    }
}