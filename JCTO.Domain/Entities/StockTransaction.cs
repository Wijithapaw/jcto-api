using JCTO.Domain.Entities.Base;
using JCTO.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace JCTO.Domain.Entities
{
    public class StockTransaction : BaseEntity
    {
        [Required]
        public Guid StockId { get; set; }
        public StockTransactionType Type { get; set; }
        [MaxLength(50)]
        public string ToBondNo { get; set; }
        public Guid? EntryId { get; set; }
        [Required]
        public DateTime TransactionDate { get; set; }
        [MaxLength(50)]
        public double Quantity { get; set; }

        public virtual Stock Stock { get; set; }
        public virtual Entry Entry { get; set; }
    }
}
