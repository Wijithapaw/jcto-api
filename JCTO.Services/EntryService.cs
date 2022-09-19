using JCTO.Domain;
using JCTO.Domain.CustomExceptions;
using JCTO.Domain.Dtos;
using JCTO.Domain.Dtos.Base;
using JCTO.Domain.Entities;
using JCTO.Domain.Enums;
using JCTO.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace JCTO.Services
{
    public class EntryService : BaseService, IEntryService
    {
        private readonly IDataContext _dataContext;
        private readonly IStockService _stockService;

        public EntryService(IDataContext dataContext, IStockService stockService)
        {
            _dataContext = dataContext;
            _stockService = stockService;
        }

        public async Task<EntityCreateResult> CreateAsync(EntryDto dto)
        {
            var stockTxn = await _stockService.DebitForEntryAsync(dto.ToBondNo, dto.InitialQuantity, dto.EntryDate);

            var newEntry = new Entry
            {
                CustomerId = stockTxn.Stock.CustomerId,
                ProductId = stockTxn.Stock.ProductId,
                EntryNo = dto.EntryNo,
                EntryDate = dto.EntryDate,
                InitialQualtity = dto.InitialQuantity,
                RemainingQuantity = dto.InitialQuantity,
                Status = dto.Status,
                StockTransaction = stockTxn,
            };

            _dataContext.StockTransactions.Add(stockTxn);
            _dataContext.Entries.Add(newEntry);

            await _dataContext.SaveChangesAsync();

            return GetEntityCreateResult(newEntry);
        }

        public async Task<EntryDto> GetAsync(Guid id)
        {
            var entry = await _dataContext.Entries
                .Where(e => e.Id == id)
                .Select(e => new EntryDto
                {
                    ToBondNo = e.StockTransaction.DischargeTransaction.ToBondNo,
                    EntryNo = e.EntryNo,
                    EntryDate = e.EntryDate,
                    InitialQuantity = e.InitialQualtity,
                    Status = e.Status,
                    ConcurrencyKey = e.ConcurrencyKey
                }).FirstOrDefaultAsync();

            return entry;
        }

        public async Task<EntityUpdateResult> UpdateAsync(Guid id, EntryDto dto)
        {
            var entry = await _dataContext.Entries
                .Where(e => e.Id == id)
                .Include(e => e.Transactions)
                .Include(e => e.StockTransaction)
                .ThenInclude(st => st.Stock)
                .Include(e => e.StockTransaction)
                .ThenInclude(st => st.DischargeTransaction)
                .ThenInclude(dt => dt.EntryTransactions)
                .FirstAsync();

            if (entry.Status == EntryStatus.Completed)
                throw new JCTOValidationException("Completed entries cannot be updated");

            var oldQty = entry.InitialQualtity;
            var newQty = dto.InitialQuantity;

            if (oldQty != newQty)
            {
                if (entry.Transactions.Any())
                    throw new JCTOValidationException("Can't update the quantity of an entry where there approvals and/or order releases");

                if (newQty < oldQty)
                {
                    entry.StockTransaction.Quantity = newQty;
                    entry.StockTransaction.Stock.RemainingQuantity += (oldQty - newQty);
                }
                else
                {
                    var addition = newQty - oldQty;
                    var remQtyOfTobond = entry.StockTransaction.DischargeTransaction.Quantity
                        + entry.StockTransaction.DischargeTransaction.EntryTransactions.Sum(t => t.Quantity);

                    if (remQtyOfTobond < addition)
                        throw new JCTOValidationException($"Remaining quantity: {remQtyOfTobond} of ToBond: {entry.StockTransaction.DischargeTransaction.ToBondNo}, not sufficient to make this change");

                    entry.StockTransaction.Quantity = newQty;
                    entry.StockTransaction.Stock.RemainingQuantity -= (newQty - oldQty);
                }

                entry.InitialQualtity = newQty;
                entry.RemainingQuantity = newQty;
            }

            entry.EntryDate = dto.EntryDate;
            entry.EntryNo = dto.EntryNo;
            entry.ConcurrencyKey = dto.ConcurrencyKey;

            await _dataContext.SaveChangesAsync();

            return GetEntityUpdateResult(entry);
        }

        public async Task DeleteAsync(Guid id)
        {
            var entry = await _dataContext.Entries
                .Where(e => e.Id == id)
                .Include(e => e.Transactions)
                .Include(e => e.StockTransaction)
                .ThenInclude(st => st.Stock)
                .FirstAsync();

            if (entry.Status == EntryStatus.Completed)
                throw new JCTOValidationException("Completed entries cannot be deleted");

            if (entry.Transactions.Any())
                throw new JCTOValidationException("Can't delete an entry when there are approvals and/or order releases");

            _dataContext.StockTransactions.Remove(entry.StockTransaction);

            entry.StockTransaction.Stock.RemainingQuantity += entry.RemainingQuantity;

            _dataContext.Entries.Remove(entry);

            await _dataContext.SaveChangesAsync();
        }

        public async Task<List<EntryTransaction>> CreateOrderEntryTransactionsAsync(string entryNo, Order order, List<OrderStockReleaseEntryDto> releaseEntries)
        {
            var entry = await GetEntryByEntryNoAsync(entryNo);

            FillObRefs(entry, releaseEntries);

            var newTxns = releaseEntries
                .Select(e => EntryTransactionService.GetEntryTransaction(EntryTransactionType.Out, e.Id, entry, order, order.OrderDate,
                                                                        null, null, e.ApprovalId, e.ObRef, e.Quantity,
                                                                        order.Status == OrderStatus.Delivered ? e.DeliveredQuantity : null))
                .ToList();

            newTxns.ForEach(t => t.Entry = entry);

            return newTxns;
        }

        private void FillObRefs(Entry entry, List<OrderStockReleaseEntryDto> releaseEntries)
        {
            if (!releaseEntries.Any(t => string.IsNullOrEmpty(t.ObRef)))
                return;

            try
            {
                var lastRefNoGiven = entry.Transactions
                    .Where(t => t.Type == EntryTransactionType.Out)
                    .Select(t => t.ObRef)
                    .Union(releaseEntries.Where(t => !string.IsNullOrEmpty(t.ObRef)).Select(t => t.ObRef))
                    .Distinct()
                    .Select(r => r.Replace($"{entry.Index}-", ""))
                    .Select(r => int.Parse(r))
                    .OrderByDescending(r => r)
                    .FirstOrDefault();

                foreach (var entryTxn in releaseEntries.Where(e => string.IsNullOrEmpty(e.ObRef)))
                {
                    entryTxn.ObRef = $"{entry.Index}-{++lastRefNoGiven}";
                }
            }
            catch (Exception ex)
            {
                throw new JCTOException($"OB Refs of entry {entry.EntryNo} are not in proper order. Hence unable to auto generate OB Refs. Manually enter OB Refs to proceed.", ex);
            }
        }

        public async Task<PagedResultsDto<EntryListItemDto>> SearchEntriesAsync(EntrySearchDto filter)
        {
            filter.EntryNo = filter.EntryNo?.ToLower().Trim() ?? "";
            filter.ToBondNo = filter.ToBondNo?.ToLower().Trim() ?? "";

            var entries = await _dataContext.Entries
                .Where(e => (filter.CustomerId == null || filter.CustomerId == e.CustomerId)
                    && (filter.ProductId == null || filter.ProductId == e.ProductId)
                    && (filter.From == null || e.EntryDate >= filter.From)
                    && (filter.To == null || e.EntryDate <= filter.To)
                    && (filter.Active == null || filter.Active.Value && e.Status == EntryStatus.Active || !filter.Active.Value && e.Status == EntryStatus.Completed)
                    && (filter.ToBondNo == "" || e.StockTransaction.DischargeTransaction.ToBondNo.ToLower() == filter.ToBondNo)
                    && (filter.EntryNo == "" || e.EntryNo.ToLower() == filter.EntryNo))
                .OrderBy(o => o.EntryDate)
                .ThenBy(o => o.EntryNo)
                .Select(e => new EntryListItemDto
                {
                    Id = e.Id,
                    EntryDate = e.EntryDate,
                    EntryNo = e.EntryNo,
                    ToBondNo = e.StockTransaction.DischargeTransaction.ToBondNo,
                    Customer = e.Customer.Name,
                    Product = e.Product.Code,
                    InitialQuantity = e.InitialQualtity,
                    RemainingQuantity = e.RemainingQuantity,
                    Status = e.Status,
                    Index = e.Index,
                    Transactions = e.Transactions
                        .OrderBy(t => t.TransactionDate)
                        .ThenBy(t => t.ObRef)
                        .Select(t => new EntryTransactionDto
                        {
                            OrderNo = t.Order != null ? t.Order.OrderNo : null,
                            TransactionDate = t.TransactionDate,
                            ApprovalType = t.Type == EntryTransactionType.Approval ? t.ApprovalType : t.ApprovalTransaction.ApprovalType,
                            ApprivalId = t.Type == EntryTransactionType.Out ? t.ApprovalTransactionId : null,
                            ApprovalRef = t.Type == EntryTransactionType.Approval ? t.ApprovalRef : t.ApprovalTransaction.ApprovalRef,
                            Type = t.Type,
                            OrderStatus = t.Order != null ? t.Order.Status : null,
                            ObRef = t.ObRef,
                            Quantity = t.Quantity,
                            DeliveredQuantity = t.DeliveredQuantity,
                        }).ToList()
                }).GetPagedListAsync(filter);

            return entries;
        }

        public async Task<EntryBalanceQtyDto> GetEntryBalanceQuantitiesAsync(string entryNo)
        {
            var balQty = await _dataContext.Entries
                .Where(e => e.EntryNo == entryNo)
                .Select(e => new EntryBalanceQtyDto
                {
                    Id = e.Id,
                    EntryNo = e.EntryNo,
                    RemainingQty = e.RemainingQuantity,
                    InitialQty = e.InitialQualtity,
                    Xbond = e.Transactions.Where(t => t.ApprovalType == ApprovalType.XBond)
                            .Select(e => e.DeliveredQuantity ?? e.Quantity)
                            .Sum(),
                    Rebond = e.Transactions.Where(t => t.ApprovalType == ApprovalType.Rebond)
                            .Select(e => e.DeliveredQuantity ?? e.Quantity)
                            .Sum(),
                    Letter = e.Transactions.Where(t => t.ApprovalType == ApprovalType.Letter)
                            .Select(e => e.DeliveredQuantity ?? e.Quantity)
                            .Sum()

                }).FirstOrDefaultAsync();

            return balQty;
        }

        public async Task<List<EntryRemaningApprovalsDto>> GetEntryRemainingApprovalsAsync(string entryNo, Guid? excludeOrderId = null)
        {
            var remApprovals = await _dataContext.EntryTransactions
                .Where(t => t.Entry.EntryNo == entryNo && t.Type == EntryTransactionType.Approval)
                .Select(t => new EntryRemaningApprovalsDto
                {
                    Id = t.Id,
                    ApprovalType = t.ApprovalType.Value,
                    ApprovalRef = t.ApprovalRef,
                    EntryNo = t.Entry.EntryNo,
                    RemainingQty = t.Quantity + t.Deliveries
                        .Where(d => excludeOrderId == null || d.OrderId != excludeOrderId)
                        .Sum(d => d.Order.Status == OrderStatus.Delivered ? d.DeliveredQuantity.Value : d.Quantity)
                })
                .Where(a => a.RemainingQty > 0)
                .ToListAsync();
            return remApprovals;
        }

        public async Task<EntityCreateResult> AddApprovalAsync(EntryApprovalDto dto)
        {
            await ValidateEntryApprovalAsync(dto);

            var entryTxn = EntryTransactionService.GetEntryTransaction(EntryTransactionType.Approval, Guid.Empty,
                null, null, dto.ApprovalDate, dto.Type, dto.ApprovalRef, null, string.Empty, dto.Quantity, null);

            entryTxn.EntryId = dto.EntryId;

            _dataContext.EntryTransactions.Add(entryTxn);
            await _dataContext.SaveChangesAsync();

            return GetEntityCreateResult(entryTxn);
        }

        public async Task DeleteApprovalAsync(Guid id)
        {
            var approvalTxn = await _dataContext.EntryTransactions
                .Where(t => t.Id == id)
                .Include(t => t.Deliveries)
                .FirstAsync(t => t.ApprovalTransactionId == id);

            if (approvalTxn.Deliveries.Any())
                throw new JCTOValidationException("Can't delete an entry approval when there are order releases associated with it");

            _dataContext.EntryTransactions.Remove(approvalTxn);
            await _dataContext.SaveChangesAsync();
        }

        public async Task<Entry> GetEntryByEntryNoAsync(string entryNo)
        {
            var entry = await _dataContext.Entries
                .Where(e => e.EntryNo == entryNo)
                .Include(e => e.Transactions)
                .FirstAsync();
            return entry;
        }

        public async Task UpdateRemainingAmountsAsync(List<Guid> entryIds)
        {
            var entries = await _dataContext.Entries
                .Where(e => entryIds.Contains(e.Id))
                .Include(e => e.Transactions).ThenInclude(t => t.Order)
                .ToListAsync();

            foreach (var entry in entries)
            {
                var totalDeliveringQty = entry.Transactions
                    .Where(t => t.Type == EntryTransactionType.Out)
                    .Select(t => t.DeliveredQuantity ?? t.Quantity)
                    .Sum();

                if (totalDeliveringQty > entry.InitialQualtity)
                    throw new JCTOValidationException($"Cant create order. Insufficient amount remaining in Entry: {entry.EntryNo}");

                entry.RemainingQuantity = entry.InitialQualtity + totalDeliveringQty; //totalDeliveringQty is minus here

                if (entry.RemainingQuantity == 0)
                {
                    var allOrders = entry.Transactions
                        .Where(t => t.Type == EntryTransactionType.Out)
                        .Select(t => t.Order)
                        .Distinct();

                    if (!allOrders.Any(o => o.Status == OrderStatus.Undelivered))
                        entry.Status = EntryStatus.Completed;
                    else
                        entry.Status = EntryStatus.Active;
                }
                else
                {
                    entry.Status = EntryStatus.Active;
                }
            }

            await _dataContext.SaveChangesAsync();
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
    }
}
