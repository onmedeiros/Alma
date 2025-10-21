using Alma.Modules.Widgets.Models;
using Alma.Modules.Widgets.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Alma.Modules.Widgets.Services
{
    public interface IWidgetRegistry
    {
        List<WidgetDescriptor> GetByContainer(string container);

        WidgetDescriptor GetByTypeName(string typeName);
    }

    public class WidgetRegistry : IWidgetRegistry
    {
        private readonly ILogger<WidgetRegistry> _logger;
        private readonly WidgetRegistryOptions _options;

        public WidgetRegistry(ILogger<WidgetRegistry> logger, IOptions<WidgetRegistryOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public List<WidgetDescriptor> GetByContainer(string container)
        {
            return _options.Widgets.Where(x => string.IsNullOrEmpty(x.Container) || x.Container == container)
                .ToList();
        }

        public WidgetDescriptor GetByTypeName(string typeName)
        {
            return _options.Widgets.FirstOrDefault(x => x.Type.FullName == typeName)
                   ?? throw new InvalidOperationException($"Widget with type name '{typeName}' is not registered.");
        }
    }
}