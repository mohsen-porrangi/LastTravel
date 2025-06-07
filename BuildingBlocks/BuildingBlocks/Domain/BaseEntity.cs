using BuildingBlocks.Contracts;

namespace BuildingBlocks.Domain
{
    public abstract class BaseEntity<T> : IEntity<T>
    {
        public T Id { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        protected BaseEntity() { }

        protected BaseEntity(T id)
        {
            Id = id;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
