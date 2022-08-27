using JCTO.Domain.Entities.Base;
using JCTO.Domain.Enums;
using System.ComponentModel.DataAnnotations;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace JCTO.Domain.Entities
{
    public class EntryTransaction : BaseEntity
    {
        [Required]
        public Guid EntryId { get; set; }
        public Guid? OrderId { get; set; }
        [Required]
        public DateTime TransactionDateTimeUtc { get; set; }
        [MaxLength(50)]
        public string ObRef { get; set; }
        public double Quantity { get; set; }
        public double DeliveredQuantity { get; set; }
        public EntryTransactionType Type { get; set; }

        public virtual Entry Entry { get; set; }
        public virtual Order Order { get; set; }
    }
}
