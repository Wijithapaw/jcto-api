using JCTO.Domain;
using JCTO.Domain.CustomExceptions;
using JCTO.Domain.Dtos;
using JCTO.Domain.Dtos.Base;
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

        public async Task<PagedResultsDto<EntryListItemDto>> SearchEntriesAsync(EntrySearchDto filter)
        {
            filter.EntryNo = filter.EntryNo?.ToLower().Trim() ?? "";

            var entries = await _dataContext.Entries
                .Where(e => (filter.CustomerId == null || filter.CustomerId == e.CustomerId)
                    && (filter.ProductId == null || filter.ProductId == e.ProductId)
                    && (filter.From == null || e.EntryDate >= filter.From)
                    && (filter.To == null || e.EntryDate <= filter.To)
                    && (!filter.ActiveEntriesOnly || e.Status == EntryStatus.Active)
                    && (filter.EntryNo == "" || e.EntryNo.ToLower().Contains(filter.EntryNo)))
                .OrderBy(o => o.EntryDate)
                .ThenBy(o => o.EntryNo)
                .Select(e => new EntryListItemDto
                {
                    Id = e.Id,
                    EntryDate = e.EntryDate,
                    EntryNo = e.EntryNo,
                    Customer = e.Customer.Name,
                    Product = e.Product.Code,
                    InitialQuantity = e.InitialQualtity,
                    RemainingQuantity = e.RemainingQuantity,
                    Status = e.Status,
                    Transactions = e.Transactions
                        .Where(t => t.Type == EntryTransactionType.Out)
                        .OrderBy(t => t.Order.OrderDate)
                        .ThenBy(t => t.Order.OrderNo)
                        .Select(t => new EntryTransactionDto
                        {
                            OrderNo = t.Order.OrderNo,
                            OrderDate = t.Order.OrderDate,
                            OrderStatus = t.Order.Status,
                            ObRef = t.ObRef,
                            Quantity = -1 * t.Quantity,
                            DeliveredQuantity = -1 * t.DeliveredQuantity
                        }).ToList()
                }).GetPagedListAsync(filter);

            return entries;
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
            var totalGoingOut = txns.Sum(t => t.Quantity);
            var remainingQty = entry.RemainingQuantity + totalGoingOut;

            if (remainingQty < 0)
                throw new JCTOValidationException($"Cant create order. Insufficient amount remaining in Entry: {entry.EntryNo}");

            entry.RemainingQuantity = remainingQty;
        }
    }
}
