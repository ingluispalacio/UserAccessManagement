namespace UserAccessManagement.Domain.Common
{
    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();

        public DateTime CreatedAt { get; private set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; private set; }
        public DateTime? DeletedAt { get; private set; }

        protected void MarkAsUpdated() => UpdatedAt = DateTime.UtcNow;
        protected void MarkAsDeleted() => DeletedAt = DateTime.UtcNow;
    }
}
