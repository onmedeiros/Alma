using Alma.Workflows.Core.Abstractions;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alma.Workflows.Core.Description.Descriptors
{
    public class ActivityDescriptor : IParameterizableDescriptor
    {
        /// <summary>
        /// The full qualified type name of the activity class.
        /// </summary>
        public required string TypeName { get; set; }

        /// <summary>
        /// Activity namespace.
        /// </summary>
        public required string Namespace { get; set; }

        /// <summary>
        /// Activity category.
        /// </summary>
        public required string Category { get; set; }

        /// <summary>
        /// Activity identifier name.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Acitivity's Namespace and Name joined.
        /// </summary>
        public required string FullName { get; set; }

        /// <summary>
        /// Activity display name.
        /// </summary>
        public required string DisplayName { get; set; }

        /// <summary>
        /// Activity description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Indicates whether user interaction is required for the operation. If true, the user must respond to proceed.
        /// </summary>
        public bool RequireInteraction { get; set; }

        /// <summary>
        /// Attributes of the activity class.
        /// </summary>
        public ICollection<object> Attributes { get; set; } = [];

        /// <summary>
        /// Parameters of the activity.
        /// </summary>
        public ICollection<ParameterDescriptor> Parameters { get; set; } = [];

        /// <summary>
        /// Ports of the activity.
        /// </summary>
        public ICollection<PortDescriptor> Ports { get; set; } = [];

        /// <summary>
        /// Data properties of the activity.
        /// </summary>
        public ICollection<DataDescriptor> DataProperties { get; set; } = [];

        /// <summary>
        /// Activity Type.
        /// </summary>
        [NotMapped]
        public required Type Type { get; set; }
    }
}