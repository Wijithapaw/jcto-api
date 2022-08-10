using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JCTO.Domain.Entities.Base
{
    public class BaseEntity : IAuditedEntity, IConcurrencyHandledEntity
    {
        public Guid? Id { get; set; }
        [Required]
        [ForeignKey("CreatedBy")]
        public Guid? CreatedById { get; set; }        
        public DateTime CreatedDateUtc { get; set; }
        [Required]
        [ForeignKey("LastUpdatedBy")]
        public Guid? LastUpdatedById { get; set; }
        public DateTime LastUpdatedDateUtc { get; set; }
        [Required]
        public Guid? ConcurrencyKey { get; set; }

        public virtual User? CreatedBy { get; set; }
        public virtual User? LastUpdatedBy { get; set; }
    }
}
