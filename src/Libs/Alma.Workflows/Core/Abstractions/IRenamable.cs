namespace Alma.Workflows.Core.Abstractions
{
    public interface IRenamable
    {
        string GetCustomName();
        void SetCustomName(string name);
    }
}
