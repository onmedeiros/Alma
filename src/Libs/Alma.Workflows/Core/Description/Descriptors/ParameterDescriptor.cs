using System.ComponentModel.DataAnnotations.Schema;

namespace Alma.Workflows.Core.Description.Descriptors
{
    public class ParameterDescriptor
    {
        public required string Name { get; set; }
        public required string DisplayName { get; set; }
        public string? DisplayValue { get; set; }
        public required string ValueType { get; set; }
        public ICollection<object> Attributes { get; set; } = [];

        [NotMapped]
        public required Type Type { get; set; }
    }
}
