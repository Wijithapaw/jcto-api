using JCTO.Domain;
using JCTO.Domain.CustomExceptions;
using JCTO.Domain.Dtos;
using JCTO.Domain.Entities;
using JCTO.Domain.Enums;
using JCTO.Domain.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCTO.Services
{
    public class EntryService : BaseService, IEntryService
    {
        private readonly IDataContext _dataContext;

        public EntryService(IDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<EntityCreateResult> CreateAsync(EntryDto dto)
        {
            var newEntry = new Entry
            {
                CustomerId = dto.CustomerId,
                ProductId = dto.ProductId,
                EntryNo = dto.EntryNo,
                EntryDate = dto.EntryDate,
                InitialQualtity = dto.InitialQuantity,
                RemainingQuantity = dto.InitialQuantity,
                Status = dto.Status,
            };

            newEntry.Transactions = new List<EntryTransaction>
            {
                EntryTransactionService.GetEntryTransaction(EntryTransactionType.In, Guid.NewGuid(), Guid.Empty, string.Empty, dto.InitialQuantity, dto.InitialQuantity)
            };

            _dataContext.Entries.Add(newEntry);

            await _dataContext.SaveChangesAsync();

            return GetEntityCreateResult(newEntry);
        }

        public async Task<List<EntryTransaction>> CreateOrderEntryTransactionsAsync(string entryNo, List<OrderStockReleaseEntryDto> releaseEntries)
        {
            var entry = await GetEntryByEntryNoAsync(entryNo);

            var newTxns = releaseEntries
                .Select(e => EntryTransactionService.GetEntryTransaction(EntryTransactionType.Out, e.Id, entry.Id, e.ObRef, e.Quantity, e.DeliveredQuantity))
                .ToList();

            UpdateRemainingAmount(entry, newTxns);

            return newTxns;
        }

        private async Task<Entry> GetEntryByEntryNoAsync(string entryNo)
        {
            var entry = await _dataContext.Entries
                .Where(e => e.EntryNo == entryNo)
                .FirstAsync();
            return entry;
        }

        private void UpdateRemainingAmount(Entry entry, List<EntryTransaction> txns)
        {
            var totalGoingOut = txns.Sum(t => t.DeliveredQuantity);
            var remainingQty = entry.RemainingQuantity + totalGoingOut;

            if (remainingQty < 0)
                throw new JCTOValidationException($"Cant create order. Insufficient amount remaining in Entry: {entry.EntryNo}");

            entry.RemainingQuantity = remainingQty;
            entry.Status = remainingQty == 0 ? EntryStatus.Completed : EntryStatus.Active;
        }
    }
}
