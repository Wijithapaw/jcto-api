namespace JCTO.Domain.Entities.Base
{
    public interface IAuditedEntity
    {
        public Guid CreatedById { get; set; }
        public DateTime CreatedDateUtc { get; set; }
        public Guid LastUpdatedById { get; set; }
        public DateTime LastUpdatedDateUtc { get; set; }
    }
}
