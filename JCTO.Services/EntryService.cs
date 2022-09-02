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

            _dataContext.Entries.Add(newEntry);

            await _dataContext.SaveChangesAsync();

            return GetEntityCreateResult(newEntry);
        }

        public async Task<List<EntryTransaction>> CreateOrderEntryTransactionsAsync(string entryNo, DateTime orderDate, List<OrderStockReleaseEntryDto> releaseEntries)
        {
            var entry = await GetEntryByEntryNoAsync(entryNo);

            var newTxns = releaseEntries
                .Select(e => EntryTransactionService.GetEntryTransaction(EntryTransactionType.Out, e.Id, entry.Id, orderDate, e.ObRef, e.Quantity, e.DeliveredQuantity))
                .ToList();

            newTxns.ForEach(t => t.Entry = entry);

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

        public async Task<EntityCreateResult> AddApprovalAsync(EntryApprovalDto dto)
        {
            await ValidateEntryApprovalAsync(dto);

            var entryTxn = new EntryTransaction
            {
                ApprovalType = dto.Type,
                ApprovalRef = dto.ApprovalRef,
                TransactionDate = dto.ApprovalDate,
                Quantity = dto.Quantity,
                DeliveredQuantity = dto.Quantity,
                Type = EntryTransactionType.Approval,
                EntryId = dto.EntryId,
            };

            _dataContext.EntryTransactions.Add(entryTxn);
            await _dataContext.SaveChangesAsync();

            return GetEntityCreateResult(entryTxn);
        }

        private async Task ValidateEntryApprovalAsync(EntryApprovalDto dto)
        {
            var errors = new List<string>();
            if ((dto.Type == ApprovalType.Rebond || dto.Type == ApprovalType.XBond) && string.IsNullOrWhiteSpace(dto.ApprovalRef))
            {
                errors.Add("Approval Ref. is required for Xbond and Rebond approvals");
            }

            var quantityToBeApproved = await _dataContext.Entries
                .Where(e => e.Id == dto.EntryId)
                .Select(e => new
                {
                    IninitalQty = e.InitialQualtity,
                    ApprovedQty = e.Transactions.Where(t => t.Type == EntryTransactionType.Approval).Sum(t => t.Quantity)
                }).Select(e => e.IninitalQty - e.ApprovedQty).FirstAsync();

            if (dto.Quantity > quantityToBeApproved)
            {
                errors.Add("Approving quantity is greater than remaining amount to approve");
            }

            if (errors.Any())
            {
                throw new JCTOValidationException(string.Join(", ", errors));
            }
        }

        private async Task<Entry> GetEntryByEntryNoAsync(string entryNo)
        {
            var entry = await _dataContext.Entries
                .Where(e => e.EntryNo == entryNo)
                .Include(e => e.Transactions)
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
