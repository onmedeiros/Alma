using Alma.Core.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alma.Modules.Widgets.Entities
{
    [Table("widgets.Widget")]
    public class Widget : Entity
    {
        public string? OrganizationId { get; set; }
        public required string Container { get; set; }
        public required string Type { get; set; }
        public required int X { get; set; }
        public required int Y { get; set; }
        public required int Width { get; set; }
        public required int Height { get; set; }
    }
}