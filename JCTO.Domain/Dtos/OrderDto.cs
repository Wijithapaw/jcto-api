using JCTO.Domain.Dtos.Base;
using JCTO.Domain.Enums;

namespace JCTO.Domain.Dtos
{
    public class OrderDto : AuditedEntityDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid ProductId { get; set; }
        public DateTime OrderDate { get; set; }
        public int OrderNo { get; set; }
        public string Buyer { get; set; }
        public OrderStatus Status { get; set; }
        public decimal Quantity { get; set; }
        public decimal? DeliveredQuantity { get; set; }
        public string ObRefPrefix { get; set; }
        public string TankNo { get; set; }
        public BuyerType BuyerType { get; set; }
        public string Remarks { get; set; }
        public bool TaxPaid { get; set; }
        public DateTime? IssueStartTime { get; set; }
        public DateTime? IssueEndTime { get; set; }

        public List<OrderStockReleaseEntryDto> ReleaseEntries { get; set; } = new List<OrderStockReleaseEntryDto>();
        public List<BowserEntryDto> BowserEntries { get; set; } = new List<BowserEntryDto>();
    }

    public class OrderStockReleaseEntryDto
    {
        public Guid Id { get; set; }
        public string EntryNo { get; set; }
        public string ObRef { get; set; }
        public decimal Quantity { get; set; }
        public decimal? DeliveredQuantity { get; set; }
        public Guid ApprovalId { get; set; }
    }

    public class BowserEntryDto
    {
        public Guid Id { get; set; }
        public int Capacity { get; set; }
        public int Count { get; set; }
    }
}
