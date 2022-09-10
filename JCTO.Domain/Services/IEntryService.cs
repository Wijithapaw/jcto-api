using JCTO.Domain.Dtos;
using JCTO.Domain.Dtos.Base;
using JCTO.Domain.Entities;

namespace JCTO.Domain.Services
{
    public interface IEntryService
    {
        Task<EntityCreateResult> CreateAsync(EntryDto dto);
        Task<List<EntryTransaction>> CreateOrderEntryTransactionsAsync(string entryNo, Order order, List<OrderStockReleaseEntryDto> releaseEntries);
        Task<PagedResultsDto<EntryListItemDto>> SearchEntriesAsync(EntrySearchDto filter);
        Task<EntityCreateResult> AddApprovalAsync(EntryApprovalDto dto);
        Task<EntryBalanceQtyDto> GetEntryBalanceQuantitiesAsync(string entryNo);
        Task UpdateRemainingAmountsAsync(List<Guid> entryIds);
        Task<Entry> GetEntryByEntryNoAsync(string entryNo);
    }
}
