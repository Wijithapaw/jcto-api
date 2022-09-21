using JCTO.Domain.Dtos.Base;
using JCTO.Domain.Enums;

namespace JCTO.Domain.Dtos
{
    public class EntryDto : ConcurrencyHandledDto
    {
        public Guid CustomerId { get; set; }
        public Guid ProductId { get; set; }
        public string EntryNo { get; set; }
        public double InitialQuantity { get; set; }
        public DateTime EntryDate { get; set; }
        public EntryStatus Status { get; set; }
    }
}
