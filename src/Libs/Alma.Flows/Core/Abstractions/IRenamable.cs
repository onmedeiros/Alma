namespace Alma.Flows.Core.Abstractions
{
    public interface IRenamable
    {
        string GetCustomName();
        void SetCustomName(string name);
    }
}
