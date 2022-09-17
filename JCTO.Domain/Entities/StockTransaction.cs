using JCTO.Domain.Entities.Base;
using JCTO.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JCTO.Domain.Entities
{
    public class StockTransaction : BaseEntity
    {
        [Required]
        [ForeignKey("Stock")]
        public Guid StockId { get; set; }
        public StockTransactionType Type { get; set; }
        [MaxLength(50)]
        public string ToBondNo { get; set; }
        [ForeignKey("DischargeTransaction")]
        public Guid? DischargeTransactionId { get; set; }
        [Required]
        public DateTime TransactionDate { get; set; }
        public double Quantity { get; set; }

        public Entry Entry { get; set; }

        public virtual StockTransaction DischargeTransaction { get; set; }
        public virtual Stock Stock { get; set; }

        public ICollection<StockTransaction> EntryTransactions { get; set; }
    }
}
