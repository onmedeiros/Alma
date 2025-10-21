using Alma.Workflows.Core.Abstractions;

namespace Alma.Workflows.Core.Description.Descriptors
{
    public class ApprovalAndCheckDescriptor : IParameterizableDescriptor
    {
        /// <summary>
        /// The full qualified type name of the Approval and Check class.
        /// </summary>
        public required string TypeName { get; set; }

        /// <summary>
        /// Approval and Check namespace.
        /// </summary>
        public required string Namespace { get; set; }

        /// <summary>
        /// Approval and Check name.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Approval and Check full name.
        /// </summary>
        public string FullName => $"{Namespace}.{Name}";

        /// <summary>
        /// Approval and Check display name.
        /// </summary>
        public required string DisplayName { get; set; }

        /// <summary>
        /// Approval and Check description.
        /// </summary>
        public string? Description { get; set; }

        public required bool CanBeApprovedManually { get; set; }

        public required ICollection<ParameterDescriptor> Parameters { get; set; } = [];

        /// <summary>
        /// Approval and Check Type.
        /// </summary>
        public required Type Type { get; set; }
    }
}