using JCTO.Domain.Entities.Base;
using JCTO.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace JCTO.Domain.Entities
{
    public class Order : BaseEntity
    {
        [Required]
        public int OrderNo { get; set; }
        [Required]
        public DateTime OrderDate { get; set; }
        [Required]
        public Guid CustomerId { get; set; }
        [Required]
        public Guid ProductId { get; set; }
        [Required]
        [MaxLength(200)]
        public string Buyer { get; set; }
        public OrderStatus Status { get; set; }
        public double Quantity { get; set; }
        public double? DeliveredQuantity { get; set; }
        [Required]
        [MaxLength(50)]
        public string ObRefPrefix { get; set; }
        [Required]
        [MaxLength(50)]
        public string TankNo { get; set; }
        public BuyerType BuyerType { get; set; }
        [MaxLength(1000)]
        public string Remarks { get; set; }
        public bool TaxPaid { get; set; }
        public DateTime? IssueStartTime { get; set; }
        public DateTime? IssueEndTime { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual Product Product { get; set; }

        public ICollection<EntryTransaction> Transactions { get; set; }
        public ICollection<BowserEntry> BowserEntries { get; set; }
    }

    public class BowserEntry : BaseEntity
    {
        public Guid OrderId { get; set; }
        public double Capacity { get; set; }
        public double Count { get; set; }

        public virtual Order Order { get; set; }
    }
}
