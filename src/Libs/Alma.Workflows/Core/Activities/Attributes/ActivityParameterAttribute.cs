namespace Alma.Workflows.Core.Activities.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ActivityParameterAttribute : Attribute
    {
        public string? DisplayName { get; set; }
        public string? DisplayValue { get; set; }
        public bool Disabled { get; set; }
        public string[] DisabledCondition { get; set; } = [];
        public string[] EnabledCondition { get; set; } = [];
        public string[] HiddenCondition { get; set; } = [];
        public bool AutoGrow { get; set; }
        public int Lines { get; set; } = 1;
        public int MaxLines { get; set; } = 1;


        public ActivityParameterAttribute(string displayName)
        {
            DisplayName = displayName;
        }

        public ActivityParameterAttribute()
        {

        }
    }
}
