using Alma.Workflows.Core.Activities.Abstractions;
using Alma.Workflows.Registries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Builders
{
    public interface IFlowBuilder
    {
        IActivityBuilder<T> AddActivity<T>() where T : class, IActivity;

        IActivityBuilder<T> AddActivity<T>(string id) where T : class, IActivity;

        IActivityBuilder AddActivity(string fullName, string id, string? discriminator = null);

        IActivityBuilder<T> AddStart<T>() where T : class, IActivity;

        IActivityBuilder<T> AddStart<T>(string id) where T : class, IActivity;

        IActivityBuilder AddStart(string fullName, string id);

        void SetStart(string id);

        FlowBuilder AddConnection(Action<ConnectionBuilder> configure);

        Flow Build();
    }

    public class FlowBuilder : IFlowBuilder
    {
        private readonly Flow _flow;
        private readonly ILogger<FlowBuilder> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IActivityRegistry _activityRegistry;

        public FlowBuilder(ILogger<FlowBuilder> logger, IServiceProvider serviceProvider, IActivityRegistry activityRegistry)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _activityRegistry = activityRegistry;

            // Construção básica do flow
            _flow = new Flow();
        }

        public IActivityBuilder<T> AddStart<T>() where T : class, IActivity
        {
            return AddStart<T>(Guid.NewGuid().ToString());
        }

        public IActivityBuilder<T> AddStart<T>(string id) where T : class, IActivity
        {
            var activityBuilder = AddActivity<T>(Guid.NewGuid().ToString());

            _flow.Start = activityBuilder.Build();

            return activityBuilder;
        }

        public IActivityBuilder AddStart(string fullName, string id)
        {
            var activityBuilder = AddActivity(fullName, id);

            _flow.Start = activityBuilder.Build();

            return activityBuilder;
        }

        public void SetStart(string id)
        {
            _flow.Start = _flow.Activities.First(x => x.Id == id);
        }

        public IActivityBuilder<T> AddActivity<T>() where T : class, IActivity
        {
            return AddActivity<T>(Guid.NewGuid().ToString());
        }

        public IActivityBuilder<T> AddActivity<T>(string id) where T : class, IActivity
        {
            var activityBuilder = _serviceProvider.GetRequiredService<IActivityBuilder<T>>();

            activityBuilder.Begin(id);

            _flow.Activities.Add(activityBuilder.Build());

            return activityBuilder;
        }

        public IActivityBuilder AddActivity(string fullName, string id, string? discriminator = null)
        {
            var activityBuilder = _serviceProvider.GetRequiredService<IActivityBuilder>();

            activityBuilder.Begin(fullName, id, discriminator);

            _flow.Activities.Add(activityBuilder.Build());

            return activityBuilder;
        }

        public FlowBuilder AddConnection(Action<ConnectionBuilder> configure)
        {
            var connectionBuilder = new ConnectionBuilder(_flow);

            configure(connectionBuilder);

            if (!connectionBuilder.IsValid)
                throw new InvalidOperationException("Connection is not valid.");

            _flow.Connections.Add(connectionBuilder.Connection);

            return this;
        }

        public Flow Build()
        {
            return _flow;
        }
    }
}