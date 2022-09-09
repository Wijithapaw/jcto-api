using JCTO.Domain.Dtos.Base;

namespace JCTO.Domain.Dtos
{
    public class StockDischargeSearchDto : PagedSearchDto
    {
        public Guid? CustomerId { get; set; }
        public Guid? ProductId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
