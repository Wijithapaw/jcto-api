using JCTO.Domain.Enums;

namespace JCTO.Domain.Dtos
{
    public class EntryTransactionDto
    {
        public Guid Id { get; set; }
        public DateTime TransactionDate { get; set; }
        public EntryTransactionType Type { get; set; }
        public ApprovalType? ApprovalType { get; set; }
        public Guid? ApprovalId { get; set; }
        public string ApprovalRef { get; set; }
        public int? OrderNo { get; set; }
        public string ObRef { get; set; }
        public double Quantity { get; set; }
        public double? DeliveredQuantity { get; set; }
        public OrderStatus? OrderStatus { get; set; }
    }
}
