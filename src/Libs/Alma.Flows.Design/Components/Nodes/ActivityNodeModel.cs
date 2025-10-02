using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using Alma.Flows.Core.Activities.Attributes;
using Alma.Flows.Core.Activities.Base;
using Alma.Flows.Core.Description.Descriptors;
using Alma.Flows.Definitions;
using Alma.Flows.Design.Components.Ports;
using Alma.Flows.Extensions;

namespace Alma.Flows.Design.Components.Nodes
{
    public class ActivityNodeModel : NodeModel
    {
        public ActivityDefinition Activity { get; init; }
        public ActivityDescriptor ActivityDescriptor { get; init; }
        public ICollection<ActivityPortModel> ActivityPorts { get; set; } = [];
        public Dictionary<string, string> ActivityParameters { get; set; } = new();

        public event Action<ActivityNodeModel>? OnActivityChanged;
        public event Action<ActivityNodeModel>? OnActivitySelected;

        #region Customizations

        public string? Icon { get; set; }
        public string? BorderColor { get; set; }

        #endregion

        public ActivityNodeModel(string id, ActivityDescriptor activityDescriptor, ActivityDefinition activity, Point? position = null) : base(id, position)
        {
            ActivityDescriptor = activityDescriptor;
            Activity = activity;

            // Carrega as customizações
            if (activityDescriptor.Attributes.FirstOrDefault(x => x.GetType() == typeof(ActivityCustomizationAttribute)) is ActivityCustomizationAttribute customizationAttribute)
            {
                Icon = customizationAttribute.Icon;
                BorderColor = customizationAttribute.BorderColor;
            }

            // Adiciona as portas de saída;
            foreach (var activityPort in activityDescriptor.Ports)
            {
                var portType = activityPort.Type switch
                {
                    PortType.Input => ActivityPortType.In,
                    PortType.Output => ActivityPortType.Out,
                    _ => ActivityPortType.Out
                };

                var portAlignment = activityPort.Type switch
                {
                    PortType.Input => PortAlignment.Left,
                    PortType.Output => PortAlignment.Right,
                    _ => PortAlignment.Right
                };

                var portCustomization = activityPort.Attributes.FirstOrDefault(x => x.GetType() == typeof(PortCustomizationAttribute)) as PortCustomizationAttribute;

                var port = new ActivityPortModel(this, type: portType, alignment: portAlignment)
                {
                    Name = activityPort.Name,
                    DisplayName = activityPort.DisplayName,
                    Color = portCustomization?.Color
                };

                AddActivityPort(port);
            }

            // Carrega os valores dos parâmetros
            foreach (var parameter in activityDescriptor.Parameters)
            {
                ActivityParameters.Add(parameter.Name, activity.Parameters.FirstOrDefault(x => x.Name == parameter.Name)?.ValueString ?? string.Empty);
            }

            Moved += OnMove;
        }

        public void AddActivityPort(ActivityPortModel port)
        {
            AddPort(port);
            ActivityPorts.Add(port);
        }

        public void ChangeParameterValue(string name, string value)
        {
            var parameter = Activity.Parameters.FirstOrDefault(x => x.Name == name);

            if (parameter is null)
            {
                var parameterDescriptor = ActivityDescriptor.Parameters.First(x => x.Name == name);

                parameter = new ParameterDefinition
                {
                    Name = name,
                    ValueString = value,
                    ValueType = parameterDescriptor.Type.ToString()
                };

                Activity.Parameters.Add(parameter);
            }

            parameter.ValueString = value;

            OnActivityChanged?.Invoke(this);
        }

        public string GetParameterValue(string name)
        {
            return Activity.Parameters.FirstOrDefault(x => x.Name == name)?.ValueString ?? string.Empty;
        }

        public void ChangeCustomDisplayName(string displayName)
        {
            Activity.CustomDisplayName = displayName;
            OnActivityChanged?.Invoke(this);
        }

        public void OnMove(MovableModel position)
        {
            Activity.SetMetadata("PosX", Position.X);
            Activity.SetMetadata("PosY", Position.Y);

            OnActivityChanged?.Invoke(this);
        }
    }
}
