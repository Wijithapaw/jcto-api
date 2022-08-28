using JCTO.Domain.Dtos.Base;
using JCTO.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCTO.Domain.Dtos
{
    public class OrderSearchDto : PagedSearchDto
    {
        public Guid? CustomerId { get; set; }
        public Guid? ProductId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string Buyer { get; set; }
        public OrderStatus? Status { get; set; }
        public BuyerType? BuyerType { get; set; }
        public string OrderNo { get; set; }
    }
}
