namespace Alma.Workflows.Core.Activities.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ActivityParameterProviderAttribute : Attribute
    {
        public Type ProviderType { get; set; }

        public ActivityParameterProviderAttribute(Type providerType)
        {
            ProviderType = providerType;
        }
    }
}