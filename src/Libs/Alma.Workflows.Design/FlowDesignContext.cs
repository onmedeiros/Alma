using Blazor.Diagrams;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using Blazor.Diagrams.Core.Routers;
using Alma.Workflows.Core.Activities.Attributes;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Core.Description.Descriptors;
using Alma.Workflows.Customizations;
using Alma.Workflows.Definitions;
using Alma.Workflows.Design.Components.Nodes;
using Alma.Workflows.Design.Components.Ports;
using Alma.Workflows.Design.Enums;
using Alma.Workflows.Enums;
using Alma.Workflows.Extensions;
using Alma.Workflows.Options;
using Alma.Workflows.Parsers;
using Alma.Workflows.Registries;
using Alma.Workflows.Runners;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Design
{
    public class FlowDesignContext
    {
        private readonly ILogger<FlowDesignContext> _logger;
        private readonly IActivityRegistry _activityRegistry;
        private readonly ICustomActivityRegistry _customActivityRegistry;
        private readonly IFlowDefinitionParser _flowDefinitionParser;
        private readonly IFlowRunnerFactory _flowRunnerFactory;

        // Fields
        private FlowDesignOptions _options = null!;

        private FlowDefinition _definition = null!;
        private BlazorDiagram _diagram = null!;
        private List<ActivityDescriptor> _availableActivities = [];
        private IEnumerable<IGrouping<string, ActivityDescriptor>> _groupedActivities = null!;

        // Properties
        public FlowDesignOptions Options => _options;

        public FlowDefinition Definition => _definition;
        public BlazorDiagram Diagram => _diagram;
        public IEnumerable<ActivityDescriptor> AvailableActivities => _availableActivities;
        public IEnumerable<IGrouping<string, ActivityDescriptor>> GroupedActivities => _groupedActivities;
        public ExecutionMode ExecutionMode { get; set; }

        #region State properties

        // States
        public FlowDesignContextExecutionStatus DesignExecutionStatus { get; private set; }

        public bool AutoSaveEnabled { get; private set; }
        public ActivityNodeModel? SelectedNode { get; private set; }
        public FlowExecutionContext? ExecutionContext => Runner?.Context;
        public FlowRunner? Runner { get; private set; }

        // Mouse states
        public bool IsDragging { get; private set; }

        public string DraggingActivityName { get; private set; } = default!;

        #endregion

        #region Events

        public event Action? OnAutoSave = null;

        public event Action? OnStateHasChanged = null;

        public event Action? OnPublish = null;

        public event Action<ActivityNodeModel?>? OnActivitySelectionChanged = null;

        public event Action<ActivityNodeModel>? OnActivitySelected = null;

        #endregion

        public FlowDesignContext(ILogger<FlowDesignContext> logger, IActivityRegistry activityRegistry, ICustomActivityRegistry customActivityRegistry, IFlowDefinitionParser flowDefinitionParser, IFlowRunnerFactory flowRunnerFactory)
        {
            _logger = logger;
            _activityRegistry = activityRegistry;
            _customActivityRegistry = customActivityRegistry;
            _flowDefinitionParser = flowDefinitionParser;
            _flowRunnerFactory = flowRunnerFactory;
        }

        public async ValueTask Initialize(FlowDesignOptions options, FlowDefinition definition)
        {
            _options = options;
            _definition = definition;

            _diagram = DiagramFactory.Create(options);

            await LoadAvailableActivities();

            LoadActivities();
            LoadConnections();

            // Load definition parameters
            AutoSaveEnabled = definition.GetMetadata<bool>("AutoSave");

            var zoom = Definition.GetMetadata<double>("Zoom");
            if (zoom > 0)
                _diagram.SetZoom(zoom);

            var panX = Definition.GetMetadata<double>("PanX");
            var panY = Definition.GetMetadata<double>("PanY");
            _diagram.SetPan(panX, panY);

            // Register events
            _diagram.ZoomChanged += HandleZoonChanged; // TODO: Revisar por conta do consumo de CPU
            _diagram.PanChanged += HandlePanChanged; // TODO: Revisar por conta do consumo de CPU
            _diagram.SelectionChanged += HandleSelectionChanged;
            _diagram.Nodes.Removed += HandleNodeRemoved;
            _diagram.Links.Added += HandleLinkAdded;
            _diagram.Links.Removed += HandleLinkRemoved;

            _logger.LogInformation("Flow Design Context initialized with definition {DefinitionId}.", definition.Id);
        }

        #region Design event handlers

        public void HandleDragStart(string activityName)
        {
            IsDragging = true;
            DraggingActivityName = activityName;
        }

        public void HandleDragDrop(DragEventArgs e)
        {
            if (string.IsNullOrEmpty(DraggingActivityName))
                return;

            var activityDescriptor = AvailableActivities.First(x => x.FullName == DraggingActivityName); // _activityRegistry.GetActivityDescriptor(DraggingActivityName);

            AddNode(activityDescriptor, _diagram.GetRelativeMousePoint(e.ClientX, e.ClientY));

            IsDragging = false;
            DraggingActivityName = string.Empty;
        }

        public void HandleZoonChanged()
        {
            Definition.SetMetadata("Zoom", _diagram.Zoom);

            OnAutoSave?.Invoke();
        }

        public void HandlePanChanged()
        {
            Definition.SetMetadata("PanX", _diagram.Pan.X);
            Definition.SetMetadata("PanY", _diagram.Pan.Y);

            OnAutoSave?.Invoke();
        }

        public void HandleSelectionChanged(SelectableModel model)
        {
            if (model is ActivityNodeModel node && node.Selected)
            {
                if (SelectedNode is null || SelectedNode.Id != node.Id)
                {
                    SelectedNode = node;
                    OnActivitySelectionChanged?.Invoke(node);
                }
            }
            else if (SelectedNode != null)
            {
                SelectedNode = null;
                OnActivitySelectionChanged?.Invoke(null);
            }
        }

        public void HandleAutoSaveChanged(bool value)
        {
            AutoSaveEnabled = value;
            Definition.SetMetadata("AutoSave", value);
            OnAutoSave?.Invoke();
        }

        public void HandleNodeRemoved(NodeModel node)
        {
            if (node is ActivityNodeModel activityNode)
            {
                Definition.Activities.Remove(activityNode.Activity);
            }

            OnAutoSave?.Invoke();
        }

        public void HandleLinkAdded(BaseLinkModel link)
        {
            if (link.Source.Model is ActivityPortModel source)
            {
                if (source.Parent is ActivityNodeModel sourceParent)
                {
                    _logger.LogInformation("Trying to add link: from {SourceType} - {SourcePort}", sourceParent.Activity.Name, source.Name);
                    link.TargetAttached += HandleLinkTargetAttached;
                }
            }
        }

        public void HandleLinkTargetAttached(BaseLinkModel link)
        {
            if (link.Target.Model is ActivityPortModel target)
            {
                if (target.Parent is ActivityNodeModel targetParent)
                {
                    var source = link.Source.Model as ActivityPortModel;
                    var sourceParent = source?.Parent as ActivityNodeModel;

                    link.TargetAttached -= HandleLinkTargetAttached;

                    _logger.LogInformation("Trying to add link: to {TargetType} - {TargetPort}", targetParent.Activity.Name, target.Name);

                    var connection = new ConnectionDefinition
                    {
                        Id = link.Id,
                        SourceActivityId = sourceParent.Activity.Id,
                        SourceActivityPort = source.Name,
                        TargetActivityId = targetParent.Activity.Id,
                        TargetActivityPort = target.Name
                    };

                    Definition.Connections.Add(connection);

                    // Experimental
                    // link.PathGenerator = new AlmaPathGenerator();
                    link.Router = new OrthogonalRouter();

                    OnAutoSave?.Invoke();
                }
            }
        }

        public void HandleLinkRemoved(BaseLinkModel link)
        {
            _logger.LogInformation("Link removed: {Id}", link.Id);

            var connection = Definition.Connections.FirstOrDefault(x => x.Id == link.Id);

            if (connection != null)
            {
                Definition.Connections.Remove(connection);
                OnAutoSave?.Invoke();
            }
        }

        private void HandleActivityChanged(ActivityNodeModel node)
        {
            OnAutoSave?.Invoke();
        }

        #endregion

        #region Design Methods

        public void AddNode(ActivityDescriptor activityDescriptor, Point point)
        {
            var activity = ActivityDefinition.Create(activityDescriptor);

            activity.SetMetadata("PosX", point.X);
            activity.SetMetadata("PosY", point.Y);

            var node = new ActivityNodeModel(activity.Id, activityDescriptor, activity, point);
            node.OnActivityChanged += HandleActivityChanged;

            _diagram.Nodes.Add(node);
            Definition.Activities.Add(activity);
        }

        #endregion

        #region Load Methods

        public async ValueTask LoadAvailableActivities()
        {
            _availableActivities.AddRange(await _customActivityRegistry.ListActivityDescriptorsAsync(_definition.Discriminator));

            if (!Options.Namespaces.Any())
            {
                _availableActivities.AddRange(_activityRegistry.ActivityDescriptors);
            }
            else
            {
                foreach (var @namespace in Options.Namespaces)
                {
                    _availableActivities.AddRange(_activityRegistry.ActivityDescriptors.Where(x => x.Namespace.StartsWith(@namespace)));
                }

                // Include activities from the definition that are not in the available activities
                foreach (var @namespace in Definition.Activities.Select(x => x.Namespace).Distinct())
                {
                    if (@namespace == "Alma.Workflows.Core.CustomActivities")
                        continue;

                    var activity = _activityRegistry.ActivityDescriptors.FirstOrDefault(x => x.Namespace == @namespace);

                    if (activity != null)
                    {
                        if (!_availableActivities.Contains(activity))
                        {
                            _availableActivities.Add(activity);
                        }
                    }
                    else
                    {
                        _logger.LogError("Namespace {Namespace} not found in the activity registry.", @namespace);
                        throw new Exception($"Namespace {@namespace} not found in the activity registry.");
                    }
                }
            }

            _groupedActivities = _availableActivities.GroupBy(x => x.Category).OrderBy(o => o.Key);
        }

        public void LoadActivities()
        {
            foreach (var activity in Definition.Activities)
                LoadNode(activity);
        }

        public void LoadConnections()
        {
            foreach (var connection in Definition.Connections)
                LoadLink(connection);
        }

        public void LoadNode(ActivityDefinition activity)
        {
            var activityDescriptor = _availableActivities.First(x => x.FullName == activity.FullName);

            var posX = activity.GetMetadata<double>("PosX");
            var posY = activity.GetMetadata<double>("PosY");

            var position = new Point(posX, posY);

            var node = new ActivityNodeModel(activity.Id, activityDescriptor, activity, position);
            node.OnActivityChanged += HandleActivityChanged;

            _diagram.Nodes.Add(node);
        }

        public void LoadLink(ConnectionDefinition connection)
        {
            var source = _diagram.Nodes.FirstOrDefault(x => x.Id == connection.SourceActivityId) as ActivityNodeModel;
            var target = _diagram.Nodes.FirstOrDefault(x => x.Id == connection.TargetActivityId) as ActivityNodeModel;

            var sourcePort = source?.ActivityPorts.FirstOrDefault(x => x.Name == connection.SourceActivityPort);
            var targetPort = target?.ActivityPorts.FirstOrDefault(x => x.Name == connection.TargetActivityPort);

            if (sourcePort != null && targetPort != null)
            {
                var link = new LinkModel(connection.Id, sourcePort, targetPort);

                // Experimental
                // link.PathGenerator = new AlmaPathGenerator();
                link.Router = new OrthogonalRouter();

                _diagram.Links.Add(link);
            }
        }

        #endregion

        #region Execution Methods

        public async Task Execute()
        {
            if (!_flowDefinitionParser.TryParse(Definition, out var flow))
            {
                throw new Exception("Failed to parse flow definition.");
            }

            var options = new ExecutionOptions
            {
                Delay = 400,
                MaxDegreeOfParallelism = 4
            };

            Runner = _flowRunnerFactory.Create(flow, options: options);

            _logger.LogInformation("Executing flow definition {FlowId}.", Definition.Id);

            DesignExecutionStatus = FlowDesignContextExecutionStatus.Executing;
            OnStateHasChanged?.Invoke();

            UpdateLinkColorsExperimental();

            await ExecuteNext();
        }

        public async Task ExecuteNext()
        {
            if (Runner is null)
                throw new Exception("Runner not initialized.");

            var hasReadyActivities = await Runner.HasNext();

            if (!hasReadyActivities)
            {
                throw new Exception("Não há atividades prontas para execução.");
            }
            else if (ExecutionMode == ExecutionMode.Automatic)
            {
                while (await Runner.ExecuteNextAsync())
                {
                    UpdateLinkColorsExperimental();

                    OnStateHasChanged?.Invoke();
                }
            }
            else
            {
                await Runner.ExecuteNextAsync();
            }

            DesignExecutionStatus = Runner.Context.State.Status switch
            {
                ExecutionStatus.Executing => FlowDesignContextExecutionStatus.Executing,
                ExecutionStatus.Waiting => FlowDesignContextExecutionStatus.Waiting,
                ExecutionStatus.Completed => FlowDesignContextExecutionStatus.Completed,
                ExecutionStatus.Failed => FlowDesignContextExecutionStatus.Failed,
                _ => FlowDesignContextExecutionStatus.NotStarted
            };

            UpdateLinkColorsExperimental();
            OnStateHasChanged?.Invoke();
        }

        public void Stop()
        {
            Runner = null;
            DesignExecutionStatus = FlowDesignContextExecutionStatus.NotStarted;

            UpdateLinkColorsExperimental();
            OnStateHasChanged?.Invoke();
        }

        public void UpdateLinkColors()
        {
            foreach (var link in _diagram.Links)
            {
                if (link is LinkModel model)
                {
                    var isExecuted = ExecutionContext!.State.ExecutedConnections.Any(x => x.ConnectionId == model.Id);

                    if (isExecuted)
                    {
                        if (model.Color != FlowColors.Success)
                        {
                            model.Color = FlowColors.Success;
                            model.Refresh();
                        }
                    }
                    else
                    {
                        if (model.Color != FlowColors.LightGray)
                        {
                            model.Color = FlowColors.LightGray;
                            model.Refresh();
                        }
                    }
                }
            }
        }

        public void UpdateLinkColorsExperimental()
        {
            foreach (var link in _diagram.Links)
            {
                if (link is LinkModel model)
                {
                    if (ExecutionContext is null)
                    {
                        model.Color = FlowColors.Black;
                        model.Refresh();

                        continue;
                    }

                    var isExecuted = ExecutionContext.State.ExecutedConnections.Any(x => x.ConnectionId == model.Id);

                    if (isExecuted)
                    {
                        var connection = Definition.Connections.First(x => x.Id == model.Id);
                        var activity = Definition.Activities.First(x => x.Id == connection.SourceActivityId);

                        ActivityDescriptor? activityDescriptor = null!;

                        if (activity.FullName.StartsWith("Alma.Workflows.Core.CustomActivities"))
                        {
                            activityDescriptor = Task.Run(async () => await _customActivityRegistry.GetActivityDescriptorAsync(activity.FullName)).GetAwaiter().GetResult();
                        }
                        else
                        {
                            activityDescriptor = _activityRegistry.GetActivityDescriptor(activity.FullName);
                        }

                        if (activityDescriptor is null)
                        {
                            _logger.LogError("Activity descriptor not found for activity {ActivityId}.", activity.Id);
                            continue;
                        }

                        var portCustomizationAttribute = activityDescriptor.Ports.First(x => x.Name == connection.SourceActivityPort).Attributes.FirstOrDefault(x => x.GetType() == typeof(PortCustomizationAttribute)) as PortCustomizationAttribute;
                        var color = portCustomizationAttribute?.Color ?? FlowColors.Success;

                        if (model.Color != color)
                        {
                            model.Color = color;
                            model.Refresh();
                        }
                    }
                    else
                    {
                        if (model.Color != FlowColors.LightGray)
                        {
                            model.Color = FlowColors.LightGray;
                            model.Refresh();
                        }
                    }
                }
            }
        }

        #endregion
    }
}