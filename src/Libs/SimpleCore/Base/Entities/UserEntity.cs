namespace Alma.Core.Entities
{
    /// <summary>
    /// An Entity owned by a specific Application User.
    /// </summary>
    public abstract class UserEntity : Entity
    {
        /// <summary>
        /// Id of the User that Entity are owned by.
        /// </summary>
        public string? OwnerId { get; set; }

        /// <summary>
        /// Id of the User that Entity are created by.
        /// </summary>
        public string? CreatorId { get; set; }
    }
}