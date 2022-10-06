using JCTO.Domain.Enums;

namespace JCTO.Domain.Dtos
{
    public class EntryListItemDto
    {
        public Guid Id { get; set; }
        public string EntryNo { get; set; }
        public decimal InitialQuantity { get; set; }
        public decimal RemainingQuantity { get; set; }
        public string Product { get; set; }
        public DateTime EntryDate { get; set; }
        public string Customer { get; set; }
        public EntryStatus Status { get; set; }
        public List<EntryTransactionDto> Transactions { get; set; }
        public int Index { get; set; }
        public string RebondedFromCustomer { get; set; }
    }
}
