using JCTO.Domain.Enums;

namespace JCTO.Domain.Dtos
{
    public class OrderListItemDto
    {
        public Guid Id { get; set; }
        public string Customer { get; set; }
        public string Product { get; set; }
        public decimal Quantity { get; set; }
        public decimal? DeliveredQuantity { get; set; }
        public DateTime OrderDate { get; set; }
        public int OrderNo { get; set; }
        public string Buyer { get; set; }
        public BuyerType BuyerType { get; set; }
        public OrderStatus Status { get; set; }
        public bool TaxPaid { get; set; }
        public DateTime? IssueStartTime { get; set; }
        public DateTime? IssueEndTime { get; set; }
        public List<BowserEntryDto> BowserEntries { get; set; }
    }
}
