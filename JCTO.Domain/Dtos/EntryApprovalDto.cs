using JCTO.Domain.Dtos.Base;
using JCTO.Domain.Enums;

namespace JCTO.Domain.Dtos
{
    public class EntryApprovalDto : ConcurrencyHandledDto
    {
        public Guid EntryId { get; set; }
        public ApprovalType Type { get; set; }
        public string ApprovalRef { get; set; }
        public double Quantity { get; set; }
        public DateTime ApprovalDate { get; set; }
    }
}
