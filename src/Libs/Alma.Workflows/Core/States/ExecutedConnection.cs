using Alma.Core.Attributes;
using Alma.Workflows.Core.Activities.Abstractions;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.ApprovalsAndChecks.Models;

namespace Alma.Workflows.States
{
    public class ExecutedConnection
    {
        public string Id { get; set; }
        public string ConnectionId { get; set; }
        public string SourceId { get; set; }
        public string SourcePortName { get; set; }
        public string TargetId { get; set; }
        public string TargetPortName { get; set; }
        public DateTime DateTime { get; set; }
        public ValueObject? Data { get; set; }

        #region Navigation properties

        [Navigation]
        public Connection Connection { get; set; }

        [Navigation]
        public IActivity Source { get; set; }

        [Navigation]
        public IActivity Target { get; set; }

        #endregion

        public ExecutedConnection(Connection connection, ValueObject? data)
        {
            Connection = connection;
            ConnectionId = connection.Id;

            Source = connection.Source.Activity;
            SourceId = connection.Source.Activity.Id;
            SourcePortName = connection.Source.PortName;

            Target = connection.Target.Activity;
            TargetId = connection.Target.Activity.Id;
            TargetPortName = connection.Target.PortName;

            Data = data;

            Id = Guid.NewGuid().ToString();
            DateTime = DateTime.Now;
        }
    }
}