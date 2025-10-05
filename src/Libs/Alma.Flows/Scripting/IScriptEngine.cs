namespace Alma.Flows.Scripting
{
    public interface IScriptEngine
    {
        void SetParameter(string name, object? value);

        Task<ScriptResult> ExecuteAsync(string script, CancellationToken cancellationToken);
    }
}