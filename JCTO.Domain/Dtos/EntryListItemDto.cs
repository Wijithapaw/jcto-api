using JCTO.Domain.Enums;

namespace JCTO.Domain.Dtos
{
    public class EntryListItemDto
    {
        public Guid Id { get; set; }
        public string EntryNo { get; set; }
        public string ToBondNo { get; set; }
        public double InitialQuantity { get; set; }
        public double RemainingQuantity { get; set; }
        public string Product { get; set; }
        public DateTime EntryDate { get; set; }
        public string Customer { get; set; }
        public EntryStatus Status { get; set; }
        public List<EntryTransactionDto> Transactions { get; set; }
    }
}
