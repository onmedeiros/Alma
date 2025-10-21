namespace Alma.Workflows.Core.Activities.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PortCustomizationAttribute : Attribute
    {
        public string? Color { get; set; }
    }
}
