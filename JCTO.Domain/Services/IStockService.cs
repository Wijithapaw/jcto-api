using JCTO.Domain.Dtos;
using JCTO.Domain.Dtos.Base;
using JCTO.Domain.Entities;

namespace JCTO.Domain.Services
{
    public interface IStockService
    {
        Task<PagedResultsDto<StockDischargeListItemDto>> SearchStockDischargesAsync(StockDischargeSearchDto filter);
        Task<EntityCreateResult> TopupAsync(StockTopupDto dto);
        Task DebitForEntryAsync(Guid customerId, Guid productId, Entry entry, double quantity, DateTime date);
    }
}
