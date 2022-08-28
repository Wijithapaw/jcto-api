using JCTO.Domain.Dtos.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCTO.Domain.Dtos
{
    public class EntrySearchDto : PagedSearchDto
    {
        public Guid? CustomerId { get; set; }
        public Guid? ProductId { get; set; }
        public string EntryNo { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public bool ActiveEntriesOnly { get; set; }
    }
}
