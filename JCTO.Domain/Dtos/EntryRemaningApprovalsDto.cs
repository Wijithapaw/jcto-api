using JCTO.Domain.Enums;

namespace JCTO.Domain.Dtos
{
    public class EntryRemaningApprovalsDto
    {
        public Guid Id { get; set; }
        public string EntryNo { get; set; }
        public ApprovalType ApprovalType { get; set; }
        public string ApprovalRef { get; set; }
        public double RemainingQty { get; set; }
    }
}
