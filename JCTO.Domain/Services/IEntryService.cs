﻿using JCTO.Domain.Dtos;
using JCTO.Domain.Dtos.Base;
using JCTO.Domain.Entities;

namespace JCTO.Domain.Services
{
    public interface IEntryService
    {
        Task<EntityCreateResult> CreateAsync(EntryDto dto);
        Task<List<EntryTransaction>> CreateOrderEntryTransactionsAsync(string entryNo, List<OrderStockReleaseEntryDto> releaseEntries);
        Task<PagedResultsDto<EntryListItemDto>> SearchEntriesAsync(EntrySearchDto filter);
    }
}
