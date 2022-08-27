using JCTO.Domain.Dtos;
using JCTO.Domain.Entities;

namespace JCTO.Domain.Services
{
    public interface IEntryService
    {
        Task<EntityCreateResult> CreateAsync(EntryDto dto);
        Task<List<EntryTransaction>> CreateOrderEntryTransactionsAsync(string entryNo, List<OrderStockReleaseEntryDto> releaseEntries);
    }
}
