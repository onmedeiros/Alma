using Alma.Core.Attributes;
using Alma.Workflows.Core.Activities.Abstractions;

namespace Alma.Workflows.Core.Activities.Base
{
    public class Endpoint
    {
        public string ActivityId { get; set; }
        public string PortName { get; set; }

        #region Navigation

        [Navigation]
        public IActivity Activity { get; set; }

        #endregion

        public Endpoint(IActivity activity, string portName)
        {
            Activity = activity;
            ActivityId = activity.Id;
            PortName = portName;
        }
    }
}