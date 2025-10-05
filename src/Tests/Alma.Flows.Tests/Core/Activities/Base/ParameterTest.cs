using Alma.Flows.Core.Activities.Base;
using Alma.Flows.Core.Contexts;
using Alma.Flows.Options;
using Alma.Flows.States;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Alma.Flows.Tests
{
    public class ParameterTest
    {
        private static ActivityExecutionContext CreateContext(
            Dictionary<string, object?>? parameters = null,
            Action<ExecutionState>? configureState = null)
        {
            var state = new ExecutionState();
            if (parameters is not null)
            {
                foreach (var kv in parameters)
                    state.Parameters[kv.Key] = kv.Value;
            }

            configureState?.Invoke(state);

            var services = new ServiceCollection().BuildServiceProvider();
            var options = new ExecutionOptions();

            return new ActivityExecutionContext(services, state, options);
        }

        private static void SetRawValueString<T>(Parameter<T> parameter, string value)
        {
            // Set backing field for private set property ValueString
            var field = typeof(Parameter<T>).GetField("<ValueString>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            field!.SetValue(parameter, value);
        }

        [Test]
        public void GetValue_ShouldReturnNull_ForReferenceType_WhenNotSet()
        {
            var p = new Parameter<string>();
            var ctx = CreateContext();

            var value = p.GetValue(ctx);

            Assert.That(value, Is.Null);
        }

        [Test]
        public void GetValue_ShouldReturnNull_ForValueType_WhenNotSet()
        {
            var p = new Parameter<int?>();
            var ctx = CreateContext();

            int? value = p.GetValue(ctx);

            Assert.That(value, Is.Null);
        }

        [Test]
        public void GetValue_ShouldReturnRawValue_ForString_WhenNotTemplate()
        {
            var p = new Parameter<string>("hello");
            var ctx = CreateContext();

            var value = p.GetValue(ctx);

            Assert.That(value, Is.EqualTo("hello"));
        }

        [Test]
        public void GetValue_ShouldConvert_ToInt_WhenNotTemplate()
        {
            var p = new Parameter<int>(42);
            var ctx = CreateContext();

            var value = p.GetValue(ctx);

            Assert.That(value, Is.EqualTo(42));
        }

        [Test]
        public void SetValue_ShouldSetAndClear()
        {
            var p = new Parameter<string>();
            var ctx = CreateContext();

            p.SetValue("abc");
            Assert.That(p.GetValue(ctx), Is.EqualTo("abc"));

            p.SetValue(null);
            Assert.That(p.GetValue(ctx), Is.Null);
        }

        [Test]
        public void GetValue_ShouldProcessTemplate_FromVariables_Var()
        {
            var p = new Parameter<string>("$var(total)");
            var ctx = CreateContext(configureState: s => s.SetVariable("total", 123));

            var value = p.GetValue(ctx);

            Assert.That(value, Is.EqualTo("123"));
        }

        [Test]
        public void GetValue_ShouldProcessTemplate_FromParameters_Param()
        {
            var p = new Parameter<string>("$param(name)");
            var ctx = CreateContext(new Dictionary<string, object?> { ["name"] = "John" });

            var value = p.GetValue(ctx);

            Assert.That(value, Is.EqualTo("John"));
        }

        [Test]
        public void GetValue_ShouldProcess_MixedText_AndMultipleTokens()
        {
            var p = new Parameter<string>("Hello $param(name), total: $var(total)");
            var ctx = CreateContext(new Dictionary<string, object?> { ["name"] = "John" }, s => s.SetVariable("total", 5));

            var value = p.GetValue(ctx);

            Assert.That(value, Is.EqualTo("Hello John, total: 5"));
        }

        [Test]
        public void GetValue_ShouldResolve_DottedPath_InParameters()
        {
            var p = new Parameter<string>("$param(user.name)");
            var parameters = new Dictionary<string, object?>
            {
                ["user"] = new Dictionary<string, object?> { ["name"] = "Maria" }
            };
            var ctx = CreateContext(parameters);

            var value = p.GetValue(ctx);

            Assert.That(value, Is.EqualTo("Maria"));
        }

        [Test]
        public void GetValue_ShouldProcessTemplate_ThenConvert_ToInt()
        {
            var p = new Parameter<int>();
            // Simulate template string storage for non-string T
            SetRawValueString(p, "$param(number)");

            var ctx = CreateContext(new Dictionary<string, object?> { ["number"] = 99 });

            var value = p.GetValue(ctx);

            Assert.That(value, Is.EqualTo(99));
        }

        [Test]
        public void GetValue_TemplateWithMissingKey_ShouldRenderEmpty_ThenConvertIfPossible()
        {
            var p = new Parameter<string>("Start-$param(unknown)-End");
            var ctx = CreateContext();

            var value = p.GetValue(ctx);

            // DotLiquid renders missing variables as empty string
            Assert.That(value, Is.EqualTo("Start--End"));
        }
    }
}