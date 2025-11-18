using Alma.Core.Extensions;
using Alma.Core.Utils;
using Alma.Workflows.Core.Activities.Attributes;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Activities.Models;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Customizations;
using Alma.Workflows.Databases.Common;
using Alma.Workflows.Databases.Registry;
using Alma.Workflows.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Alma.Workflows.Databases.Activities
{
    [Activity(
        Namespace = "Alma.Workflows.Databases",
        Category = "Banco de dados",
        DisplayName = "Consulta",
        Description = "Realiza uma consulta ao banco de dados com as informações configuradas.")]
    [ActivityCustomization(Icon = DatabaseIcons.Database, BorderColor = DatabaseColors.Default)]
    public class QueryDatabaseActivity : Activity
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

        [ActivityParameter(DisplayName = "Consulta", DisplayValue = "{{value}}", AutoGrow = true, Lines = 4, MaxLines = 10)]
        public Parameter<string>? Query { get; set; }

        [ActivityParameter(DisplayName = "Variável", DisplayValue = "{{value}}")]
        public Parameter<string>? Variable { get; set; }

        #endregion

        public override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var databaseProviderOption = DatabaseProvider?.GetValue(context);
            var connectionString = ConnectionString?.GetValue(context);
            var query = Query?.GetValue(context);
            var variable = Variable?.GetValue(context);

            if (databaseProviderOption is null || string.IsNullOrEmpty(databaseProviderOption.Value))
            {
                context.State.Log("Invalid Database Provider", Enums.LogSeverity.Error);
                Fail.Execute();
                return;
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                context.State.Log("Connection String is required", Enums.LogSeverity.Error);
                Fail.Execute();
                return;
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                context.State.Log("Query is required", Enums.LogSeverity.Error);
                Fail.Execute();
                return;
            }

            if (string.IsNullOrEmpty(variable))
                variable = "QueryResult";

            var databaseProviderRegistry = context.ServiceProvider.GetRequiredService<IDatabaseProviderRegistry>();
            var databaseProvider = await databaseProviderRegistry.GetProvider(databaseProviderOption.Value);

            if (databaseProvider is null)
            {
                context.State.Log($"Database Provider '{databaseProviderOption.Value}' not found", Enums.LogSeverity.Error);
                Fail.Execute();
                return;
            }

            var connectionResult = await databaseProvider.ConnectAsync(connectionString);

            if (!connectionResult.Succeeded)
            {
                context.State.Log($"Connection failed: {connectionResult.Message}", Enums.LogSeverity.Error);
                Fail.Execute();
                return;
            }

            var queryResult = await databaseProvider.QueryJsonAsync(query!);

            if (queryResult.Succeeded)
            {
                context.State.Log("Query executed successfully", Enums.LogSeverity.Information);

                var resultDictionary = JsonUtils.ConvertToDictionary(queryResult.Data ?? "{}");

                context.State.SetVariable(variable, resultDictionary);
                Done.Execute(queryResult);
            }
            else
            {
                context.State.Log($"Query failed: {queryResult.Message}", Enums.LogSeverity.Error);
                Fail.Execute(queryResult);
            }
        }
    }
}