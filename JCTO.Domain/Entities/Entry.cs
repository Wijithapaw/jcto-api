using JCTO.Domain.Entities.Base;
using JCTO.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JCTO.Domain.Entities
{
    public class Entry : BaseEntity
    {
        [Required]
        public Guid CustomerId { get; set; }
        [Required]
        public Guid ProductId { get; set; }
        [Required]
        [MaxLength(50)]
        public string EntryNo { get; set; }
        public double InitialQualtity { get; set; }
        public double RemainingQuantity { get; set; }
        public DateTime EntryDate { get; set; }
        public EntryStatus Status { get; set; }

        public StockTransaction StockTransaction { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual Product Product { get; set; }

        public ICollection<EntryTransaction> Transactions { get; set; }
    }
}
