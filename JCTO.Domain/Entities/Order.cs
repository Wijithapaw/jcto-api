using JCTO.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace JCTO.Domain.Entities
{
    public class Order: BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string OrderNo { get; set; }
        public DateTime OrderDate { get; set; }

        public ICollection<EntryTransaction> Transactions { get; set; }
    }
}
