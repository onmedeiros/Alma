using Alma.Modules.Widgets.Models;
using Microsoft.AspNetCore.Components;

namespace Alma.Modules.Widgets.Options
{
    public class WidgetRegistryOptions
    {
        private readonly List<WidgetDescriptor> _widgets = [];
        public IReadOnlyList<WidgetDescriptor> Widgets => _widgets;

        public void Register(Type type, WidgetOptions options)

        {
            if (type is null)
                throw new ArgumentNullException(nameof(type), "Widget type cannot be null.");

            var typeName = type.FullName
                ?? throw new InvalidOperationException("Widget type name is empty.");

            if (_widgets.FirstOrDefault(x => x.Type == type) is not null)
                throw new InvalidOperationException($"Widget with type name '{typeName}' is already registered.");

            _widgets.Add(new WidgetDescriptor
            {
                Type = type,
                Name = options.Name,
                Container = options.Container,
                Width = options.Width,
                Height = options.Height,
                MaxWidth = options.MaxWidth,
                MaxHeight = options.MaxHeight,
                MinWidth = options.MinWidth,
                MinHeight = options.MinHeight,
            });
        }

        public void Register<TWidget>(WidgetOptions options)
            where TWidget : ComponentBase
        {
            options.Name ??= typeof(TWidget).FullName ??
                throw new InvalidOperationException("Widget type name is empty.");

            if (string.IsNullOrWhiteSpace(options.Name))
                throw new InvalidOperationException("Impossible to define Widget name. Widget name cannot be null or empty.");

            Register(typeof(TWidget), options);
        }
    }
}