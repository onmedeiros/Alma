using Alma.Core.Types;
using Alma.Core.Utils;
using Alma.Workflows.Activities.ParameterProviders;
using Alma.Workflows.Core.Activities.Attributes;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Activities.Models;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Customizations;
using Alma.Workflows.Monitoring.Common;
using Alma.Workflows.Monitoring.Models;
using Alma.Workflows.Monitoring.MonitoringObjects.Entities;
using Alma.Workflows.Monitoring.MonitoringObjects.Services;
using Alma.Workflows.Monitoring.MonitoringObjectSchemas.ParameterProviders;
using Alma.Workflows.Monitoring.MonitoringObjectSchemas.Services;
using Alma.Organizations.Contexts;
using Microsoft.Extensions.DependencyInjection;

namespace Alma.Workflows.Monitoring.Activities
{
    [Activity(
        Namespace = "Alma.Workflows.Monitoring",
        Category = "Monitoramento",
        DisplayName = "Criar Objeto",
        Description = "Cria um novo objeto de monitoramento no banco de dados.")]
    [ActivityCustomization(Icon = MonitoringIcons.MonitoringObject, BorderColor = MonitoringColors.Default)]
    public class CreateMonitoringObjectActivity : Activity
    {
        #region Ports

        [Port(DisplayName = "Entrada", Type = PortType.Input)]
        public Port Input { get; set; } = default!;

        [Port(DisplayName = "Sucesso", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Success)]
        public Port Done { get; set; } = default!;

        [Port(DisplayName = "Falha", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Error)]
        public Port Fail { get; set; } = default!;

        #endregion

        #region Parameters

        [ActivityParameter(DisplayName = "Esquema do objeto", DisplayValue = "<b>Esquema:</b> {{value}}")]
        [ActivityParameterProvider(typeof(MonitoringObjectSchemaParameterProvider))]
        public Parameter<ParameterOption>? MonitoringObjectSchema { get; set; }

        [ActivityParameter(DisplayName = "Nome da variável", DisplayValue = "<b>Nome:</b> {{value}}")]
        public Parameter<string>? VariableName { get; set; }

        [ActivityParameter(DisplayName = "Se existir")]
        public Parameter<IfExists>? IfExists { get; set; }

        #endregion

        public override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var schemeId = MonitoringObjectSchema?.GetValue(context)?.Value;
            var name = VariableName?.GetValue(context);

            if (string.IsNullOrWhiteSpace(schemeId))
            {
                context.State.Log("Esquema do objeto não informado.", Enums.LogSeverity.Error);
                Fail.Execute();
                return;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                context.State.Log("Nome do objeto não informado.", Enums.LogSeverity.Error);
                Fail.Execute();
                return;
            }

            if (!context.State.Variables.TryGetValue(name, out var monitoringValueObject))
            {
                context.State.Log($"Variável {name} não encontrada no contexto de execução.", Enums.LogSeverity.Error);
                Fail.Execute();
                return;
            }

            try
            {
                var organizationContext = context.ServiceProvider.GetRequiredService<IOrganizationContext>();
                var monitoringObjectService = context.ServiceProvider.GetRequiredService<IMonitoringObjectService>();
                var monitoringObjectSchemaService = context.ServiceProvider.GetRequiredService<IMonitoringObjectSchemaService>();

                var organzationId = await organizationContext.TryGetCurrentOrganizationId();
                var monitoringObjectData = JsonUtils.ConvertToDictionary(monitoringValueObject.ValueString ?? "{}");
                var monitoringObjectSchema = await monitoringObjectSchemaService.GetOneAsync(schemeId);

                if (monitoringObjectData is null)
                {
                    context.State.Log("Dados do objeto de monitoramento inválidos.", Enums.LogSeverity.Error);
                    Fail.Execute();
                    return;
                }

                if (monitoringObjectSchema is null)
                {
                    context.State.Log("Esquema do objeto não encontrado.", Enums.LogSeverity.Error);
                    Fail.Execute();
                    return;
                }

                // Set primary key
                var requirePrimaryKey = monitoringObjectSchema.Fields.Any(x => x.IsPrimaryKey);
                string primaryKeyValue = string.Empty;

                if (requirePrimaryKey)
                {
                    var primaryKeyName = monitoringObjectSchema.Fields.First(x => x.IsPrimaryKey).Name;
                    var hasPrimaryKey = monitoringObjectData.TryGetValue(primaryKeyName, out var tempPrimaryKeyValue);

                    if (!hasPrimaryKey || primaryKeyValue is null || string.IsNullOrWhiteSpace(tempPrimaryKeyValue?.ToString()))
                    {
                        context.State.Log($"Chave primária '{primaryKeyName}' não informada nos dados do objeto de monitoramento.", Enums.LogSeverity.Error);
                        Fail.Execute();
                        return;
                    }

                    primaryKeyValue = tempPrimaryKeyValue!.ToString()!;
                }
                else
                {
                    // Generate a new primary key if not defined in schema
                    primaryKeyValue = Guid.NewGuid().ToString();
                }

                // Set timestamp
                var requireTimestamp = monitoringObjectSchema.Fields.Any(x => x.IsTimestamp);
                var timestampValue = DateTime.UtcNow;

                if (requireTimestamp)
                {
                    var timestampFieldName = monitoringObjectSchema.Fields.First(x => x.IsTimestamp).Name;
                    var hasTimestamp = monitoringObjectData.TryGetValue(timestampFieldName, out var tempTimestampValue);

                    if (!hasTimestamp || tempTimestampValue is null)
                    {
                        context.State.Log($"Timestamp não informado.", Enums.LogSeverity.Error);
                        Fail.Execute();
                        return;
                    }

                    if (DateTime.TryParse(tempTimestampValue.ToString(), out var parsedTimestamp))
                    {
                        timestampValue = parsedTimestamp;
                    }
                    else
                    {
                        context.State.Log($"Timestamp inválido.", Enums.LogSeverity.Error);
                        Fail.Execute();
                        return;
                    }
                }

                var exists = await monitoringObjectService.ExistsAsync(schemeId, primaryKeyValue, organzationId);
                var ifExists = IfExists?.GetValue(context) ?? Models.IfExists.ThrowError;

                var monitoringObject = new MonitoringObject
                {
                    OrganizationId = organzationId,
                    SchemaId = schemeId,
                    PrimaryKey = primaryKeyValue,
                    Timestamp = timestampValue,
                    Data = monitoringObjectData
                };

                ServiceResult<MonitoringObject> result = null!;

                if (exists && ifExists == Models.IfExists.Ignore)
                {
                    result = ServiceResult<MonitoringObject>.Success(null);
                }
                else if (exists && ifExists == Models.IfExists.Replace)
                {
                    result = await monitoringObjectService.ReplaceAsync(monitoringObject);
                }
                else
                {
                    result = await monitoringObjectService.CreateAsync(monitoringObject);
                }

                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors?.Select(e => e.Message) ?? [""]);
                    context.State.Log($"Erro ao criar o objeto de monitoramento: {errors}", Enums.LogSeverity.Error);
                    Fail.Execute();
                    return;
                }

                context.State.Log($"Objeto de monitoramento '{name}' criado com sucesso.", Enums.LogSeverity.Information);
                Done.Execute();
            }
            catch (Exception ex)
            {
                context.State.Log($"Erro ao criar o objeto de monitoramento: {ex.Message}", Enums.LogSeverity.Error);
                Fail.Execute();
            }
        }
    }
}