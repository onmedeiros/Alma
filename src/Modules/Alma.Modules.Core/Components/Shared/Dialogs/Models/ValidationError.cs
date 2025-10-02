namespace Alma.Modules.Core.Components.Shared.Dialogs.Models
{
    public class ValidationError
    {
        public string? Code { get; set; }
        public string? Message { get; set; }
        public string? PropertyName { get; set; }
        public string? AttemptedValue { get; set; }
    }
}