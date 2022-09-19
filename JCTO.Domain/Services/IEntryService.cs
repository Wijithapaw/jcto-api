using JCTO.Domain.Dtos;
using JCTO.Domain.Dtos.Base;
using JCTO.Domain.Entities;

namespace JCTO.Domain.Services
{
    public interface IEntryService
    {
        Task<EntryDto> GetAsync(Guid id);
        Task<EntityCreateResult> CreateAsync(EntryDto dto);
        Task<EntityUpdateResult> UpdateAsync(Guid id, EntryDto dto);
        Task DeleteAsync(Guid id);
        Task<List<EntryTransaction>> CreateOrderEntryTransactionsAsync(string entryNo, Order order, List<OrderStockReleaseEntryDto> releaseEntries);
        Task<PagedResultsDto<EntryListItemDto>> SearchEntriesAsync(EntrySearchDto filter);
        Task<EntityCreateResult> AddApprovalAsync(EntryApprovalDto dto);
        Task DeleteApprovalAsync(Guid id);
        Task<EntryBalanceQtyDto> GetEntryBalanceQuantitiesAsync(string entryNo);
        Task UpdateRemainingAmountsAsync(List<Guid> entryIds);
        Task<Entry> GetEntryByEntryNoAsync(string entryNo);
        Task<List<EntryRemaningApprovalsDto>> GetEntryRemainingApprovalsAsync(string entryNo, Guid? excludeOrderId = null);
    }
}
