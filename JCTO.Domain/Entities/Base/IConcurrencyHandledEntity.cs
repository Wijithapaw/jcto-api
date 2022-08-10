namespace JCTO.Domain.Entities.Base
{
    public interface IConcurrencyHandledEntity
    {
        public Guid? ConcurrencyKey { get; set; }
    }
}
