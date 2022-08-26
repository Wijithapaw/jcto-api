using JCTO.Domain.Enums;

namespace JCTO.Domain.Dtos
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid ProductId { get; set; }
        public DateTime OrderDate { get; set; }
        public string? OrderNo { get; set; }
        public string? Buyer { get; set; }
        public OrderStatus Status { get; set; }
        public double Quantity { get; set; }
        public string? ObRefPrefix { get; set; }
        public string? TankNo { get; set; }
        public BuyerType BuyerType { get; set; }
        public string? XBondNo { get; set; }
        public string? Remarks { get; set; }
        public List<OrderStockReleaseEntry> ReleaseEntries { get; set; } = new List<OrderStockReleaseEntry>();
        public List<BowserEntry> BowserEntries { get; set; } = new List<BowserEntry>();
    }

    public class OrderStockReleaseEntry
    {
        public Guid Id { get; set; }
        public string? EntryNo { get; set; }
        public string? ObRef { get; set; }
        public double Quantity { get; set; }
        public double DeliveredQuantity { get; set; }
    }

    public class BowserEntry
    {
        public Guid Id { get; set; }
        public double Capacity { get; set; }
        public double Count { get; set; }
    }
}
