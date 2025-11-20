using Alma.Workflows.Core.Activities.Attributes;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Customizations;

namespace Alma.Workflows.Activities.Flow
{
    public class ForeachAcitvity
    {
        #region Ports

        [Port(DisplayName = "Entrada", Type = PortType.Input)]
        public Port Input { get; set; } = default!;

        [Port(DisplayName = "Completo", Type = PortType.Input)]
        public Port BodyComplete { get; set; } = default!;

        [Port(DisplayName = "Corpo do Loop", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Flow)]
        public Port Body { get; set; } = default!;

        [Port(DisplayName = "Concluído", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Success)]
        public Port Done { get; set; } = default!;

        [Port(DisplayName = "Falha", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Error)]
        public Port Fail { get; set; } = default!;

        #endregion
    }
}