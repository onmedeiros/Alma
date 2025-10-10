using Alma.Flows.Core.Common.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alma.Flows.Core.InstanceEndpoints.Entities
{
    [Table("flows.InstanceEndpoint")]
    public class InstanceEndpoint
    {
        public required string Id { get; set; }
        public required string InstanceId { get; set; }
        public string? Discriminator { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastRunAt { get; set; }
        public required string Name { get; set; }
        public string? ApiId { get; set; }
        public required string Path { get; set; }
        public required ApiMethod Method { get; set; }
        public bool IsActive { get; set; }
    }
}