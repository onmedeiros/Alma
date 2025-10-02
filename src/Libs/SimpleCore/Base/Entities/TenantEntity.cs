namespace Alma.Core.Entities
{
    /// <summary>
    /// An Entity owned by a specific Tenant.
    /// </summary>
    public abstract class TenantEntity : Entity
    {
        /// <summary>
        /// Id of the Tenant that Entity are owned by.
        /// </summary>
        public string? TenantId { get; set; }
    }
}