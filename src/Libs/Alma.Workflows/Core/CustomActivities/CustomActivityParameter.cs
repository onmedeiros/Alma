namespace Alma.Workflows.Core.CustomActivities
{
    public class CustomActivityParameter
    {
        public required string Name { get; set; }
        public object? Value { get; set; }
        public string? ValueTemplate { get; set; }

        public CustomActivityParameter()
        {
        }

        public CustomActivityParameter(object? value)
        {
            Value = value;
        }

        public bool HasTemplate => !string.IsNullOrEmpty(ValueTemplate);
    }
}