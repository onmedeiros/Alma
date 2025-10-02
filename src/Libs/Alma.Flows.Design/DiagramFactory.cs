using Blazor.Diagrams;
using Blazor.Diagrams.Core.PathGenerators;
using Blazor.Diagrams.Core.Routers;
using Blazor.Diagrams.Options;
using Alma.Flows.Design.Components.Nodes;

namespace Alma.Flows.Design
{
    public static class DiagramFactory
    {
        public static BlazorDiagram Create(FlowDesignOptions options)
        {
            var diagramOptions = new BlazorDiagramOptions
            {
                GridSize = 18,
                AllowMultiSelection = false,
                Zoom =
                {
                    Enabled = true,
                    Inverse = true,
                    Minimum = 0.4,
                    Maximum = 1.2,
                    ScaleFactor = 1.2
                },
                Links =
                {
                    DefaultRouter = new OrthogonalRouter(),
                    DefaultPathGenerator = new StraightPathGenerator(14)
                }
            };

            // Registra as restrições
            diagramOptions.Constraints.ShouldDeleteNode = (node) =>
            {
                if (node is ActivityNodeModel activityNode)
                {
                    if (activityNode.Activity.Name == "Start")
                        return ValueTask.FromResult(false);
                }

                return ValueTask.FromResult(true);
            };

            // Cria uma instância do diagrama.
            var diagram = new BlazorDiagram(diagramOptions);

            // Registra os componentes customizados
            diagram.RegisterComponent<ActivityNodeModel, ActivityNodeWidget>();

            return diagram;
        }
    }
}