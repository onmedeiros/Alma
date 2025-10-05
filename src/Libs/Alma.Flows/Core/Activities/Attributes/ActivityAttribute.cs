namespace Alma.Flows.Core.Activities.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ActivityAttribute : Attribute
    {
        public string? Namespace { get; set; }
        public string? Category { get; set; }
        public string? DisplayName { get; set; }
        public string? Description { get; set; }
        public bool RequireInteraction { get; set; } = false;

        public ActivityAttribute(string @namespace, string category)
        {
            Namespace = @namespace;
            Category = category;
        }

        public ActivityAttribute()
        {
        }
    }
}