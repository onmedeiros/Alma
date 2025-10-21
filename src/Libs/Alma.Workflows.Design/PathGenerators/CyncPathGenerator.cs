using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models.Base;
using Blazor.Diagrams.Core.PathGenerators;

namespace Alma.Workflows.Design.PathGenerators
{
    public class AlmaPathGenerator : SmoothPathGenerator
    {
        public AlmaPathGenerator(double margin = 125) : base(margin)
        {
        }

        public override PathGeneratorResult GetResult(Diagram diagram, BaseLinkModel link, Point[] route, Point source, Point target)
        {
            // Detecta se o target está "atrás" do source (à esquerda)
            if (target.X < source.X)
            {
                // Offsets mais suaves e proporcionais
                double minHorizontal = 40;
                double minVertical = 40;
                double maxHorizontal = 120;
                double maxVertical = 120;
                double horizontalOffset = Math.Clamp(Math.Abs(target.X - source.X) * 0.4, minHorizontal, maxHorizontal);
                double verticalOffset = Math.Clamp(Math.Abs(target.Y - source.Y) * 0.6, minVertical, maxVertical);
                bool goOver = target.Y < source.Y; // true = por cima, false = por baixo
                double midY = goOver ? Math.Min(source.Y, target.Y) - verticalOffset : Math.Max(source.Y, target.Y) + verticalOffset;

                var loopRoute = new[]
                {
                    source,
                    new Point(source.X + horizontalOffset, source.Y),
                    new Point(source.X + horizontalOffset, midY),
                    new Point(target.X - horizontalOffset, midY),
                    new Point(target.X - horizontalOffset, target.Y),
                    target
                };
                return base.GetResult(diagram, link, loopRoute, source, target);
            }
            // Caso padrão: smooth normal
            return base.GetResult(diagram, link, route, source, target);
        }
    }
}