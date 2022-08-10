namespace JCTO.Domain.Dtos
{
    public class EntityUpdateResult
    {
        public Guid ConcurrencyKey { get; set; }
    }

    public class EntityCreateResult
    {
        public Guid Id { get; set; }
        public Guid ConcurrencyKey { get; set; }
    }
}
