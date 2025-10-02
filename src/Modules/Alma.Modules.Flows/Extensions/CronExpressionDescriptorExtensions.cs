using CronExpressionDescriptor;
using NCrontab;

namespace Alma.Core.Domain.Shared.Extensions
{
    public static class CronExpressionDescriptorExtensions
    {
        public static bool IsValidCronExpression(this string? cronExpression)
        {
            try
            {
                ExpressionDescriptor.GetDescription(cronExpression, new Options
                {
                    ThrowExceptionOnParseError = true,
                });

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsCronAtLeastEvery(this string cronExpression, string minCronExpression)
        {
            if (!cronExpression.IsValidCronExpression())
            {
                return false;
            }

            try
            {
                var cron = CrontabSchedule.Parse(cronExpression, new CrontabSchedule.ParseOptions
                {
                    IncludingSeconds = true
                });

                var minCron = CrontabSchedule.Parse(minCronExpression, new CrontabSchedule.ParseOptions
                {
                    IncludingSeconds = true
                });

                var now = DateTime.Now;
                return cron.GetNextOccurrence(now) >= minCron.GetNextOccurrence(now);
            }
            catch
            {
                return false;
            }
        }

        public static string? GetCronDescription(this string cronExpression)
        {
            if (!string.IsNullOrEmpty(cronExpression))
            {
                return ExpressionDescriptor.GetDescription(cronExpression, new Options
                {
                    Locale = "pt-BR",
                    Use24HourTimeFormat = true
                });
            }

            return "Cron inválida";
        }
    }
}