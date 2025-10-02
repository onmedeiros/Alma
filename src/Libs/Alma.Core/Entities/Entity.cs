using System.ComponentModel.DataAnnotations;

namespace Alma.Core.Entities
{
    /// <summary>
    /// A base entity.
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// Entity identificator.
        /// </summary>
        [Key]
        public virtual string Id { get; set; } = default!;

        /// <summary>
        /// When entity created date.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Last entity modification date.
        /// </summary>
        public DateTime ModifiedAt { get; set; }

        /// <summary>
        /// If entity is deleted.
        /// </summary>
        public bool Deleted { get; set; }

        public Entity()
        {
            var now = DateTime.UtcNow;
            Id = Guid.NewGuid().ToString();
            CreatedAt = now;
            ModifiedAt = now;
            Deleted = false;
        }
    }
}