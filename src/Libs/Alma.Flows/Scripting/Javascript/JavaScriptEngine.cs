using Jint;

namespace Alma.Flows.Scripting.Javascript
{
    public class JavaScriptEngine : IScriptEngine
    {
        private readonly Dictionary<string, object?> _parameters = new Dictionary<string, object?>();

        public async Task<ScriptResult> ExecuteAsync(string script, CancellationToken cancellationToken)
        {
            var result = new ScriptResult();

            var engine = new Engine(options =>
            {
                options.LimitMemory(4_000_000); // 4MB
                options.TimeoutInterval(TimeSpan.FromSeconds(5)); // 5 seconds
                options.MaxStatements(1_000_000); // 1mi statements
                options.CancellationToken(cancellationToken); // Cancellation token
            });

            // Cria um objeto anônimo para Parameters e injeta no contexto do JS
            var parametersObj = new System.Dynamic.ExpandoObject() as IDictionary<string, object?>;

            foreach (var kvp in _parameters)
            {
                parametersObj[kvp.Key] = kvp.Value;
            }

            // Configure engine
            engine.SetValue("Parameters", parametersObj);

            engine.SetValue("log", new Action<string>(message => Log(result, message)));
            engine.SetValue("executePort", new Action<string, object?>((name, data) => ExecutePort(result, name, data)));

            try
            {
                engine.Execute(script);
                return result;
            }
            catch (Exception ex)
            {
                result.Fail(ex.Message);
                return result;
            }
        }

        public void SetParameter(string name, object? value)
        {
            _parameters[name] = value;
        }

        private void Log(ScriptResult result, string message)
        {
            result.Log(message);
        }

        private void ExecutePort(ScriptResult result, string name, object? data)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Port name cannot be null or empty.", nameof(name));
            }

            result.ExecutePort(name, data);
        }
    }
}