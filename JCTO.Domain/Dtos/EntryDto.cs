using JCTO.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCTO.Domain.Dtos
{
    public class EntryDto
    {
        public string? EntryNo { get; set; }
        public double InitialQuantity { get; set; }
        public Guid ProductId { get; set; }
        public DateTime EntryDate { get; set; }
        public Guid CustomerId { get; set; }
        public EntryStatus Status { get; set; }
    }
}
