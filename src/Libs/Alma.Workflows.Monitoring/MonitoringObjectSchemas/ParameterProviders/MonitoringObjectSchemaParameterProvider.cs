using Alma.Workflows.Core.Activities.Abstractions;
using Alma.Workflows.Core.Activities.Models;
using Alma.Workflows.Monitoring.MonitoringObjectSchemas.Models;
using Alma.Workflows.Monitoring.MonitoringObjectSchemas.Services;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Monitoring.MonitoringObjectSchemas.ParameterProviders
{
    public class MonitoringObjectSchemaParameterProvider : IParameterProvider
    {
        private readonly ILogger<MonitoringObjectSchemaParameterProvider> _logger;
        private readonly IMonitoringObjectSchemaService _monitoringObjectSchemaService;

        public MonitoringObjectSchemaParameterProvider(ILogger<MonitoringObjectSchemaParameterProvider> logger, IMonitoringObjectSchemaService monitoringObjectSchemaService)
        {
            _logger = logger;
            _monitoringObjectSchemaService = monitoringObjectSchemaService;
        }

        public async Task<IEnumerable<ParameterOption>> LoadOptionsAsync(string? term = null, string? discriminator = null)
        {
            var schemas = await _monitoringObjectSchemaService.SearchAsync(new MonitoringObjectSchemaSearchModel
            {
                PageIndex = 1,
                PageSize = int.MaxValue,
                OrganizationId = discriminator,
                Term = term
            });

            return schemas.Items.OrderBy(x => x.Name).Select(s => new ParameterOption
            {
                DisplayName = s.Name,
                Value = s.Id
            });
        }
    }
}