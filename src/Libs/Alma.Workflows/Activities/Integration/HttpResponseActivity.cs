using Alma.Workflows.Core.Activities.Attributes;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Customizations;
using Alma.Workflows.Enums;
using Alma.Workflows.Models.Activities;

namespace Alma.Workflows.Activities.Integration
{
    [Activity(
        Namespace = "Alma.Workflows",
        Category = "Integração",
        DisplayName = "Resposta HTTP",
        Description = "Cria uma resposta HTTP para chamadas de API.")]
    [ActivityCustomization(Icon = FlowIcons.Http, BorderColor = FlowColors.Integration)]
    public class HttpResponseActivity : Activity
    {
        #region Ports

        [Port(DisplayName = "Entrada", Type = PortType.Input)]
        public Port Input { get; set; } = default!;

        [Port(DisplayName = "Ok", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Success)]
        public Port Done { get; set; } = default!;

        #endregion

        #region Parameters

        [ActivityParameter(DisplayName = "Status Code", DisplayValue = "{{value}}")]
        public Parameter<HttpStatusCode>? StatusCode { get; set; }

        [ActivityParameter(DisplayName = "Conteúdo", AutoGrow = true, Lines = 3, MaxLines = 8)]
        public Parameter<string>? Content { get; set; }

        #endregion

        public override ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var statusCode = StatusCode?.GetValue(context) ?? HttpStatusCode.Ok;
            var content = Content?.GetValue(context) ?? string.Empty;

            var httpResponse = new HttpResponse
            {
                StatusCode = statusCode,
                Content = content
            };

            context.State.Variables.Set("HttpResponse", httpResponse);

            Done.Execute(httpResponse);

            return ValueTask.CompletedTask;
        }
    }
}