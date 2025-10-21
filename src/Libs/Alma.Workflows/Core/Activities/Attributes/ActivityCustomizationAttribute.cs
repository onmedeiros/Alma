namespace Alma.Workflows.Core.Activities.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ActivityCustomizationAttribute : Attribute
    {
        public string? Icon { get; set; }
        public string? BorderColor { get; set; }
    }
}
