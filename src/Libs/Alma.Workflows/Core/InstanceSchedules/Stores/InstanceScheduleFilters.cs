namespace Alma.Workflows.Core.InstanceSchedules.Stores
{
    public class InstanceScheduleFilters
    {
        public string? Name { get; set; }
        public string? Discriminator { get; set; }
        public string? InstanceId { get; set; }
    }
}
