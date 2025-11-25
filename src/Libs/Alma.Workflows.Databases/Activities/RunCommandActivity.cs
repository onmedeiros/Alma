using Alma.Core.Extensions;
using Alma.Core.Utils;
using Alma.Workflows.Core.Activities.Attributes;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Activities.Models;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Customizations;
using Alma.Workflows.Databases.Common;
using Alma.Workflows.Databases.Registry;
using Microsoft.Extensions.DependencyInjection;

namespace Alma.Workflows.Databases.Activities
{
    [Activity(
        Namespace = "Alma.Workflows.Databases",
        Category = "Banco de dados",
        DisplayName = "Executar comando",
        Description = "Realiza uma consulta ao banco de dados com as informações configuradas.")]
    [ActivityCustomization(Icon = DatabaseIcons.Database, BorderColor = DatabaseColors.Default)]
    public class RunCommandActivity : Activity
    {
        #region Ports

        [Port(DisplayName = "Entrada", Type = PortType.Input)]
        public Port Input { get; set; } = default!;

        [Port(DisplayName = "Concluído", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Success)]
        public Port Done { get; set; } = default!;

        [Port(DisplayName = "Falha", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Error)]
        public Port Fail { get; set; } = default!;

        #endregion

        #region Parameters

        [ActivityParameter(DisplayName = "Provedor do Banco de Dados", DisplayValue = "{{value}}")]
        [ActivityParameterProvider(typeof(DatabaseProviderParameterProvider))]
        public Parameter<ParameterOption>? DatabaseProvider { get; set; }

        [ActivityParameter(DisplayName = "String de Conexão", DisplayValue = "{{value}}", AutoGrow = true, Lines = 1, MaxLines = 3)]
        public Parameter<string>? ConnectionString { get; set; }

        [ActivityParameter(DisplayName = "Comando", DisplayValue = "{{value}}", AutoGrow = true, Lines = 4, MaxLines = 10)]
        public Parameter<string>? Command { get; set; }

        [ActivityParameter(DisplayName = "Variável", DisplayValue = "{{value}}")]
        public Parameter<string>? Variable { get; set; }

        #endregion

        public override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var databaseProviderOption = DatabaseProvider?.GetValue(context);
            var connectionString = ConnectionString?.GetValue(context);
            var command = Command?.GetValue(context);
            var variable = Variable?.GetValue(context);

            if (databaseProviderOption is null || string.IsNullOrEmpty(databaseProviderOption.Value))
            {
                context.State.Logs.Add("Invalid Database Provider", Enums.LogSeverity.Error);
                Fail.Execute();
                return;
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                context.State.Logs.Add("Connection String is required", Enums.LogSeverity.Error);
                Fail.Execute();
                return;
            }

            if (string.IsNullOrWhiteSpace(command))
            {
                context.State.Logs.Add("Query is required", Enums.LogSeverity.Error);
                Fail.Execute();
                return;
            }

            if (string.IsNullOrEmpty(variable))
                variable = "CommandResult";

            var databaseProviderRegistry = context.ServiceProvider.GetRequiredService<IDatabaseProviderRegistry>();
            var databaseProvider = await databaseProviderRegistry.GetProvider(databaseProviderOption.Value);

            if (databaseProvider is null)
            {
                context.State.Logs.Add($"Database Provider '{databaseProviderOption.Value}' not found", Enums.LogSeverity.Error);
                Fail.Execute();
                return;
            }

            var connectionResult = await databaseProvider.ConnectAsync(connectionString);

            if (!connectionResult.Succeeded)
            {
                context.State.Logs.Add($"Connection failed: {connectionResult.Message}", Enums.LogSeverity.Error);
                Fail.Execute();
                return;
            }

            var comandResult = await databaseProvider.RunCommandJsonAsync(command!);

            if (comandResult.Succeeded)
            {
                context.State.Logs.Add("Command executed successfully", Enums.LogSeverity.Information);

                var resultDictionary = JsonUtils.ConvertToDictionary(comandResult.Data ?? "{}");

                context.State.Variables.Set(variable, resultDictionary);
                Done.Execute(comandResult);
            }
            else
            {
                context.State.Logs.Add($"Command failed: {comandResult.Message}", Enums.LogSeverity.Error);
                Fail.Execute(comandResult);
            }
        }
    }
}