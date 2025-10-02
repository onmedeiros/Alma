using Alma.Core.Attributes;
using Alma.Flows.Core.Abstractions;

namespace Alma.Flows.Core.Activities.Base
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