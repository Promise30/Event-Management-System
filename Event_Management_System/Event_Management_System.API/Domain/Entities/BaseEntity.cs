namespace Event_Management_System.API.Domain.Entities
{
    public abstract class BaseEntity<TKey>
    {
        public TKey Id { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }

        protected BaseEntity(TKey id)
        {
            this.Id = id;
            this.CreatedDate = DateTimeOffset.UtcNow;
            this.ModifiedDate = DateTimeOffset.UtcNow;
        }

        protected BaseEntity()
        {
        }
    }
}
