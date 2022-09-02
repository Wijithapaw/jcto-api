using JCTO.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCTO.Domain.Dtos
{
    public class EntryApprovalDto
    {
        public Guid EntryId { get; set; }
        public ApprovalType Type { get; set; }
        public string ApprovalRef { get; set; }
        public double Quantity { get; set; }
        public DateTime ApprovalDate { get; set; }
    }
}
