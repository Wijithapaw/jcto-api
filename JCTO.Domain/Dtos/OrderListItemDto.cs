using JCTO.Domain.Enums;

namespace JCTO.Domain.Dtos
{
    public class OrderListItemDto
    {
        public Guid Id { get; set; }
        public string Customer { get; set; }
        public string Product { get; set; }
        public double Quantity { get; set; }
        public double? DeliveredQuantity { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderNo { get; set; }
        public string Buyer { get; set; }
        public BuyerType BuyerType { get; set; }
        public OrderStatus Status { get; set; }
        public bool TaxPaid { get; set; }
    }
}
