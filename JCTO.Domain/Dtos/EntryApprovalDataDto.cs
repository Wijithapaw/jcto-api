namespace JCTO.Domain.Dtos
{
    public class EntryApprovalSummaryDto
    {
        public Guid CustomerId { get; set; }
        public Guid ProductId { get; set; }
        public Guid ApprovalId { get; set; }
        public string TobondNo { get; set; }
    }
}
