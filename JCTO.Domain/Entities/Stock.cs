using JCTO.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations;

namespace JCTO.Domain.Entities
{
    public class Stock : BaseEntity
    {
        [Required]
        public Guid CustomerId { get; set; }
        [Required]
        public Guid ProductId { get; set; }
        public double RemainingQuantity { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual Product Product { get; set; }

        public ICollection<StockTransaction> Transactions { get; set; }
    }
}
