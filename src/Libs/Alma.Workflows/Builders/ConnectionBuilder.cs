using Alma.Workflows.Core.Activities.Abstractions;
using Alma.Workflows.Core.Activities.Base;
using System.Linq.Expressions;

namespace Alma.Workflows.Builders
{
    public class ConnectionBuilder
    {
        private readonly Flow _flow;

        public bool IsValid { get; private set; } = false;
        public Connection Connection { get; private set; }

        public ConnectionBuilder(Flow flow)
        {
            Connection = new Connection();
            _flow = flow;
        }

        public ConnectionBuilder WithId(string id)
        {
            Connection.Id = id;
            return this;
        }

        public ConnectionBuilder WithSource(Endpoint source)
        {
            Connection.Source = source;
            return this;
        }

        public ConnectionBuilder WithSource(IActivity activity, string portName)
        {
            return WithSource(new Endpoint(activity, portName));
        }

        public ConnectionBuilder WithSource<TActivity>(TActivity activity, Expression<Func<TActivity, Port>> portExpression)
            where TActivity : class, IActivity
        {
            if (portExpression.Body is MemberExpression memberExpression)
            {
                var parameterName = memberExpression.Member.Name;
                return WithSource(activity, parameterName);
            }
            else
            {
                throw new ArgumentException("Invalid port expression", nameof(portExpression));
            }
        }

        public ConnectionBuilder WithSource(string activityId, string portName)
        {
            var activity = _flow.Activities.First(x => x.Id == activityId);
            return WithSource(activity, portName);
        }

        public ConnectionBuilder WithTarget(Endpoint target)
        {
            if (Connection.Source == null)
                throw new InvalidOperationException($"Connection source is not set. Call {nameof(WithSource)} method first.");

            (var isCompatible, var message) = CheckCompatibility(Connection.Source, target);

            if (!isCompatible)
                throw new Exception(message);

            Connection.Target = target;
            IsValid = true;

            // Add navigation to the Ports
            var sourcePort = Connection.Source.Activity.GetPorts().First(x => x.Descriptor.Name == Connection.Source.PortName);
            var targetPort = Connection.Target.Activity.GetPorts().First(x => x.Descriptor.Name == Connection.Target.PortName);

            sourcePort.ConnectedPorts.Add(targetPort);
            targetPort.ConnectedPorts.Add(sourcePort);

            return this;
        }

        public ConnectionBuilder WithTarget(IActivity activity, string portName)
        {
            return WithTarget(new Endpoint(activity, portName));
        }

        public ConnectionBuilder WithTarget<TActivity>(TActivity activity, Expression<Func<TActivity, Port>> portExpression)
            where TActivity : class, IActivity
        {
            if (portExpression.Body is MemberExpression memberExpression)
            {
                var parameterName = memberExpression.Member.Name;
                return WithTarget(activity, parameterName);
            }
            else
            {
                throw new ArgumentException("Invalid port expression", nameof(portExpression));
            }
        }

        public ConnectionBuilder WithTarget(string activityId, string portName)
        {
            var activity = _flow.Activities.First(x => x.Id == activityId);
            return WithTarget(activity, portName);
        }

        private (bool compatible, string? reason) CheckCompatibility(Endpoint source, Endpoint target)
        {
            var sourceDescriptor = _flow.Activities.First(x => x.Id == source.ActivityId)
                .Descriptor
                .Ports
                .First(x => x.Name == source.PortName);

            var targetDescriptor = _flow.Activities.First(x => x.Id == target.ActivityId)
                .Descriptor
                .Ports
                .First(x => x.Name == target.PortName);

            if (sourceDescriptor.DataType is null && targetDescriptor.DataType is null)
                return (true, null);

            if (sourceDescriptor.DataType is not null && targetDescriptor.DataType is null)
                return (true, null);

            if (sourceDescriptor.DataType is not null && targetDescriptor.DataType is not null)
            {
                if (!sourceDescriptor.DataType.IsAssignableTo(targetDescriptor.DataType))
                    return (false, $"The assigned ports are not compatible. Trying to assign {sourceDescriptor.DataType} to {targetDescriptor.DataType}.");
            }

            return (true, null);
        }
    }
}