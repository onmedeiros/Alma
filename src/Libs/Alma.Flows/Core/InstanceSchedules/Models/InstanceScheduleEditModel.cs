namespace Alma.Flows.Core.InstanceSchedules.Models
{
    public class InstanceScheduleEditModel
    {
        public required string Id { get; set; }
        public string? Discriminator { get; set; }
        public string? Name { get; set; }
        public bool? IsActive { get; set; }
        public string? CronExpression { get; set; }
    }
}
