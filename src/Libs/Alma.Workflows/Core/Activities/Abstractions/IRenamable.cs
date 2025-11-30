namespace Alma.Workflows.Core.Activities.Abstractions
{
    public interface IRenamable
    {
        string GetCustomName();
        void SetCustomName(string name);
    }
}
