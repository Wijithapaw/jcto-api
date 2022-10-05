namespace JCTO.Domain.Dtos.Base
{
    public class AuditedEntityDto : ConcurrencyHandledDto
    {
        public string CreatedBy { get; set; }
        public DateTime? CreatedDateUtc { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedDateUtc { get; set; }
    }
}
