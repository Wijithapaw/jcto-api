using JCTO.Domain.Entities.Base;
using JCTO.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JCTO.Domain.Entities
{
    public class EntryTransaction : BaseEntity
    {
        [Required]
        public Guid EntryId { get; set; }
        public EntryTransactionType Type { get; set; }
        public ApprovalType? ApprovalType { get; set; }
        [MaxLength(50)]
        public string ApprovalRef { get; set; }
        [Required]
        public DateTime TransactionDate { get; set; }
        [MaxLength(50)]
        public string ObRef { get; set; }
        public decimal Quantity { get; set; }
        public decimal? DeliveredQuantity { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? ApprovalTransactionId { get; set; }

        public virtual Entry Entry { get; set; }
        public virtual Order Order { get; set; }
        public virtual EntryTransaction ApprovalTransaction { get; set; }

        [InverseProperty("RebondFromEntryTxn")]
        public Entry RebondToEntry { get; set; }

        public ICollection<EntryTransaction> Deliveries { get; set; }
    }
}
