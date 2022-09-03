using JCTO.Domain.Enums;

namespace JCTO.Domain.Dtos
{
    public class EntryTransactionDto
    {
        public DateTime TransactionDate { get; set; }
        public EntryTransactionType Type { get; set; }
        public ApprovalType ApprovalType { get; set; }
        public string ApprovalRef { get; set; }
        public string OrderNo { get; set; }
        public string ObRef { get; set; }
        public double Quantity { get; set; }
        public double? DeliveredQuantity { get; set; }
        public OrderStatus? OrderStatus { get; set; }
    }
}
