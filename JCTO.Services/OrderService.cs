using JCTO.Domain;
using JCTO.Domain.CustomExceptions;
using JCTO.Domain.Dtos;
using JCTO.Domain.Dtos.Base;
using JCTO.Domain.Entities;
using JCTO.Domain.Enums;
using JCTO.Domain.Services;
using JCTO.Reports;
using Microsoft.EntityFrameworkCore;
using NumericWordsConversion;

namespace JCTO.Services
{
    public class OrderService : BaseService, IOrderService
    {
        private readonly IDataContext _dataContext;
        private readonly IEntryService _entryService;

        public OrderService(IDataContext dataContext, IEntryService entryService)
        {
            _dataContext = dataContext;
            _entryService = entryService;
        }
        public async Task<EntityCreateResult> CreateAsync(OrderDto dto)
        {
            ValidateOrder(dto);

            await ValidateEntriesAsync(dto);

            var order = new Order
            {
                CustomerId = dto.CustomerId,
                ProductId = dto.ProductId,
                OrderDate = dto.OrderDate,
                OrderNo = dto.OrderNo,
                Buyer = dto.Buyer,
                Status = OrderStatus.Undelivered,
                Quantity = dto.Quantity,
                ObRefPrefix = dto.ObRefPrefix.Trim(),
                TankNo = dto.TankNo,
                BuyerType = dto.BuyerType,
                Remarks = dto.Remarks,
                TaxPaid = dto.TaxPaid
            };

            _dataContext.Orders.Add(order);

            var entryTxns = await CreateNewEntryTxnsAsync(dto.ReleaseEntries, order);

            _dataContext.EntryTransactions.AddRange(entryTxns);

            order.BowserEntries = dto.BowserEntries.Select(b => new BowserEntry
            {
                Capacity = b.Capacity,
                Count = b.Count,
            }).ToList();

            await _entryService.UpdateRemainingAmountsAsync(entryTxns.Select(t => t.Entry.Id).ToList());

            await _dataContext.SaveChangesAsync();

            return GetEntityCreateResult(order);
        }

        public async Task<EntityUpdateResult> UpdateAsync(Guid id, OrderDto dto)
        {
            dto.Id = id;

            var order = await _dataContext.Orders
               .Where(o => o.Id == id)
               .Include(o => o.Transactions).ThenInclude(t => t.Entry)
               .Include(o => o.BowserEntries)
               .SingleOrDefaultAsync();

            if (order.Status == OrderStatus.Delivered && dto.Status == OrderStatus.Delivered)
                throw new JCTOValidationException("Delivered orders cannot be updated");

            ValidateOrder(dto);

            var orderMarkingUndelivered = order.Status == OrderStatus.Delivered && dto.Status == OrderStatus.Undelivered;

            await ValidateEntriesAsync(dto, true, orderMarkingUndelivered);

            var oldAffectedEntryIds = order.Transactions.Select(t => t.Entry.Id).ToList();

            order.CustomerId = dto.CustomerId;
            order.ProductId = dto.ProductId;
            order.OrderDate = dto.OrderDate;
            order.OrderNo = dto.OrderNo;
            order.Buyer = dto.Buyer;
            order.Status = dto.Status;
            order.DeliveredQuantity = dto.Status == OrderStatus.Delivered ? dto.DeliveredQuantity : null;
            order.Quantity = dto.Quantity;
            order.ObRefPrefix = dto.ObRefPrefix.Trim();
            order.TankNo = dto.TankNo;
            order.BuyerType = dto.BuyerType;
            order.Remarks = dto.Remarks;
            order.ConcurrencyKey = dto.ConcurrencyKey;
            order.TaxPaid = dto.TaxPaid;
            order.IssueStartTime = dto.IssueStartTime;
            order.IssueEndTime = dto.IssueEndTime;

            //Delete Current release entries and create new ones
            _dataContext.EntryTransactions.RemoveRange(order.Transactions);

            var newEntryTxns = await CreateNewEntryTxnsAsync(dto.ReleaseEntries, order);
            _dataContext.EntryTransactions.AddRange(newEntryTxns);

            var newAffectedEntryIds = order.Transactions.Select(t => t.Entry.Id).ToList();

            //Bowser entries
            order.BowserEntries = dto.BowserEntries.Select(b => new BowserEntry
            {
                Capacity = b.Capacity,
                Count = b.Count,
            }).ToList();

            var allAffectedEntries = newAffectedEntryIds.Union(oldAffectedEntryIds).Distinct().ToList();

            await _dataContext.SaveChangesAsync();

            await _entryService.UpdateRemainingAmountsAsync(allAffectedEntries);

            return GetEntityUpdateResult(order);
        }

        public async Task DeleteAsync(Guid id)
        {
            var order = await _dataContext.Orders
                .Where(o => o.Id == id)
                .Include(o => o.Transactions)
                .Include(o => o.BowserEntries)
                .SingleOrDefaultAsync();

            if (order.Status == OrderStatus.Delivered)
                throw new JCTOValidationException("Delivered orders cannot be deleted");

            _dataContext.EntryTransactions.RemoveRange(order.Transactions);
            _dataContext.BowserEntries.RemoveRange(order.BowserEntries);
            _dataContext.Orders.Remove(order);

            var affectedEntryIds = order.Transactions.Select(t => t.EntryId).ToList();

            await _dataContext.SaveChangesAsync();

            await _entryService.UpdateRemainingAmountsAsync(affectedEntryIds);
        }

        public async Task CancelOrderAsync(Guid id)
        {
            var order = await _dataContext.Orders
                .Where(o => o.Id == id)
                .Include(o => o.Transactions)
                .ThenInclude(t => t.Entry)
                .FirstAsync();

            if (order.Status != OrderStatus.Undelivered)
                throw new JCTOValidationException("Only Undelivered orders are allowed to cancel");

            order.Status = OrderStatus.Cancelled;

            var reversals = new List<EntryTransaction>();
            foreach (var txn in order.Transactions)
            {
                var reversal = EntryTransactionService.GetEntryTransaction(EntryTransactionType.Reversal, Guid.Empty, txn.Entry, order,
                    DateTime.Now, null, null, txn.ApprovalTransactionId, null, txn.Quantity, null);
                reversals.Add(reversal);
            }

            _dataContext.EntryTransactions.AddRange(reversals);

            await _dataContext.SaveChangesAsync();

            var affectedEntryIds = order.Transactions.Select(t => t.EntryId).Distinct().ToList();

            await _entryService.UpdateRemainingAmountsAsync(affectedEntryIds);
        }

        public async Task<OrderDto> GetOrderAsync(Guid id)
        {
            var order = await _dataContext.Orders
                .Where(o => o.Id == id)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    CustomerId = o.CustomerId,
                    ProductId = o.ProductId,
                    OrderNo = o.OrderNo,
                    OrderDate = o.OrderDate,
                    Buyer = o.Buyer,
                    BuyerType = o.BuyerType,
                    ObRefPrefix = o.ObRefPrefix,
                    Quantity = o.Quantity,
                    DeliveredQuantity = o.DeliveredQuantity,
                    Status = o.Status,
                    TankNo = o.TankNo,
                    Remarks = o.Remarks,
                    TaxPaid = o.TaxPaid,
                    IssueStartTime = o.IssueStartTime,
                    IssueEndTime = o.IssueEndTime,
                    ReleaseEntries = o.Transactions
                        .Where(t => t.Type == EntryTransactionType.Out)
                        .Select(t => new OrderStockReleaseEntryDto
                        {
                            Id = t.Id,
                            EntryNo = t.Entry.EntryNo,
                            ObRef = t.ObRef,
                            DeliveredQuantity = -1 * t.DeliveredQuantity,
                            Quantity = -1 * t.Quantity,
                            ApprovalId = t.ApprovalTransactionId.Value
                        }).OrderBy(e => e.EntryNo).ToList(),
                    BowserEntries = o.BowserEntries.Select(b => new BowserEntryDto
                    {
                        Id = b.Id,
                        Capacity = b.Capacity,
                        Count = b.Count
                    }).OrderByDescending(b => b.Capacity).ToList(),
                    ConcurrencyKey = o.ConcurrencyKey
                }).FirstOrDefaultAsync();

            return order;
        }

        public async Task<PagedResultsDto<OrderListItemDto>> SearchOrdersAsync(OrderSearchDto filter)
        {
            filter.Buyer = filter.Buyer?.ToLower().Trim() ?? "";

            var orders = await _dataContext.Orders
                .Where(o => (filter.CustomerId == null || filter.CustomerId == o.CustomerId)
                    && (filter.ProductId == null || filter.ProductId == o.ProductId)
                    && (filter.From == null || o.OrderDate >= filter.From)
                    && (filter.To == null || o.OrderDate <= filter.To)
                    && (filter.Status == null || o.Status == filter.Status)
                    && (filter.BuyerType == null || o.BuyerType == filter.BuyerType)
                    && (filter.Buyer == "" || o.Buyer.ToLower().Contains(filter.Buyer))
                    && (filter.OrderNo == null || o.OrderNo == filter.OrderNo))
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.OrderNo)
                .Select(o => new OrderListItemDto
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    OrderNo = o.OrderNo,
                    Buyer = o.Buyer,
                    BuyerType = o.BuyerType,
                    Customer = o.Customer.Name,
                    Product = o.Product.Code,
                    Quantity = o.Quantity,
                    DeliveredQuantity = o.DeliveredQuantity,
                    Status = o.Status,
                    TaxPaid = o.TaxPaid,
                    IssueStartTime = o.IssueStartTime,
                    IssueEndTime = o.IssueEndTime,
                }).GetPagedListAsync(filter);

            return orders;
        }

        public async Task<int> GetNextOrderNoAsync(DateTime date)
        {
            var currentOrderNo = await _dataContext.Orders
                .Where(o => o.OrderDate == date)
                .MaxAsync(o => (int?)o.OrderNo);
            return (currentOrderNo ?? 0) + 1;
        }

        public async Task<byte[]> GenerateStockReleaseAsync(Guid orderId)
        {
            var reportData = await _dataContext.Orders
                .Where(o => o.Id == orderId)
                .Select(o => new StockReleaseReportDto
                {
                    OrderNo = o.OrderNo,
                    OrderDate = o.OrderDate.ToString("dd/MM/yyyy"),
                    Customer = o.Customer.Name,
                    Product = o.Product.Code,
                    Buyer = o.Buyer,
                    Quantity = o.Quantity,
                    EntryNo = string.Join(",", o.Transactions.Select(t => $"{t.Entry.EntryNo}{(!string.IsNullOrEmpty(t.ApprovalTransaction.ApprovalRef) ? "/" + t.ApprovalTransaction.ApprovalRef : "")}")),
                    ObRef = o.ObRefPrefix + "/" + string.Join(", ", o.Transactions.Select(t => t.ObRef)),
                    TankNo = o.TankNo,
                    Remarks = o.BuyerType == BuyerType.Bowser ? string.Join(" + ", o.BowserEntries.Select(b => $"{b.Capacity}Ltrs x {b.Count.ToString("00")}")) : "",
                    TaxPaid = o.TaxPaid
                }).SingleOrDefaultAsync();

            if (reportData.TaxPaid)
                reportData.Remarks = string.IsNullOrEmpty(reportData.Remarks) ? "Duty Paid" : $"{reportData.Remarks} - Duty paid";


            var options = new NumericWordsConversionOptions
            {
                DecimalPlaces = 3,
                DecimalSeparator = "decimal"
            };

            NumericWordsConverter converter = new NumericWordsConverter(options);
            reportData.QuantityInText = $"{converter.ToWords((decimal)reportData.Quantity)} MT of {reportData.Product} only";

            return await StockReleaseReport.GenerateAsync(reportData);
        }

        private void ValidateOrder(OrderDto order)
        {
            var errors = new List<string>();

            if (order.CustomerId == Guid.Empty)
                errors.Add("Customer not found");

            if (order.ProductId == Guid.Empty)
                errors.Add("Product not found");

            if (order.OrderDate == DateTime.MinValue)
                errors.Add("Order Date not found");

            if (order.OrderNo <= 0)
                errors.Add("Order No. not valid");

            if (string.IsNullOrWhiteSpace(order.Buyer))
                errors.Add("Buyer not found");

            if (order.Quantity <= 0)
                errors.Add("Quantity must be > 0");

            if (string.IsNullOrWhiteSpace(order.ObRefPrefix))
                errors.Add("OBRef not found");

            if (string.IsNullOrWhiteSpace(order.TankNo))
                errors.Add("Tank No. not found");

            if ((order.DeliveredQuantity ?? 0) > order.Quantity)
                errors.Add("Delivered quantity is > original quantity");

            if (!order.ReleaseEntries.Any())
            {
                errors.Add("Stock release entries not found");
            }
            else
            {
                var duplicateBonds = order.ReleaseEntries
                    .GroupBy(e => new { e.EntryNo, e.ApprovalId })
                    .Select(g => new { EntryNo = g.Key.EntryNo, Count = g.Count() })
                    .ToList();

                foreach (var entry in duplicateBonds.Where(b => b.Count > 1))
                    errors.Add($"Duplicate bond approvals found for entry: {entry.EntryNo}");

                if (order.ReleaseEntries.Sum(e => e.Quantity) != order.Quantity)
                    errors.Add("Sum of release Quantities not equal to overall Quantity");

                if (order.ReleaseEntries.Any(e => e.ApprovalId == Guid.Empty))
                    errors.Add("Stock release entries found not having Approval");

                if (order.Status == OrderStatus.Delivered)
                {
                    if (order.ReleaseEntries.Sum(e => e.DeliveredQuantity) != order.DeliveredQuantity)
                        errors.Add("Sum of Delivered Quantities not equal to overall Delivered Quantity");

                    if (order.ReleaseEntries.Any(e => e.DeliveredQuantity > e.Quantity))
                        errors.Add("Stock release entries found having Delevered Quantity > Quantity");
                }
            }

            if (order.BuyerType == BuyerType.Bowser)
            {
                if (!order.BowserEntries.Any())
                {
                    errors.Add("Bowser details not found");
                }
            }

            if (errors.Any())
            {
                throw new JCTOValidationException(string.Join(", ", errors));
            }
        }

        private async Task ValidateEntriesAsync(OrderDto order, bool update = false, bool orderMarkingUndelivered = false)
        {
            var errors = new List<string>();

            var entryNos = order.ReleaseEntries.Select(e => e.EntryNo).ToArray();

            var entries = await _dataContext.Entries
                .Where(e => entryNos.Contains(e.EntryNo))
                .Select(e => new
                {
                    e.Id,
                    e.EntryNo,
                    e.CustomerId,
                    e.ProductId,
                    RemainingQuantity = e.RemainingQuantity +
                        (update ? e.Transactions
                                      .Where(t => t.OrderId == order.Id)
                                      .Select(e => e.DeliveredQuantity ?? e.Quantity)
                                      .Sum() * -1 : 0),
                    e.InitialQualtity,
                    e.Status,
                    RemApprovedQtys = e.Transactions
                        .Where(t => t.Type == EntryTransactionType.Approval)
                        .Select(t => new
                        {
                            Id = t.Id,
                            ApprovalType = t.ApprovalType.Value,
                            t.ApprovalRef,
                            RemainingQty = t.Quantity + t.Deliveries
                                                        .Where(d => !update || d.OrderId != order.Id)
                                                        .Sum(d => d.Order.Status == OrderStatus.Delivered ? d.DeliveredQuantity.Value : d.Quantity)
                        }).ToList(),
                })
                .ToListAsync();

            var invalidEntries = entryNos
                .Except(entries.Select(e => e.EntryNo))
                .OrderBy(e => e)
                .ToArray();

            if (invalidEntries.Any())
                errors.Add($"Invalid entries: {string.Join("|", invalidEntries)}");

            var completedEntries = entries
                .Where(e => e.Status == EntryStatus.Completed)
                .Select(e => e.EntryNo)
                .OrderBy(e => e)
                .ToArray();

            if (!orderMarkingUndelivered && completedEntries.Any())
                errors.Add($"Completed entries: {string.Join("|", completedEntries)} cannot be used");

            var productMismatchs = entries
                .Where(e => e.ProductId != order.ProductId)
                .Select(e => e.EntryNo)
                .ToArray();

            if (productMismatchs.Any())
                errors.Add($"Product miss-matching entries: {string.Join("|", productMismatchs)}");

            var customerMismatchs = entries
                .Where(e => e.CustomerId != order.CustomerId)
                .Select(e => e.EntryNo)
                .ToArray();

            if (customerMismatchs.Any())
                errors.Add($"Customer miss-matching entries: {string.Join("|", customerMismatchs)}");

            foreach (var reqBondQty in order.ReleaseEntries.Where(r => !completedEntries.Union(invalidEntries).Contains(r.EntryNo)))
            {
                var entry = entries.First(e => e.EntryNo == reqBondQty.EntryNo);

                var remApprovedQty = entry.RemApprovedQtys.Find(a => a.Id == reqBondQty.ApprovalId);

                if (remApprovedQty == null)
                    errors.Add($"Incorrect approval selection for entry: {reqBondQty.EntryNo}");
                else if (remApprovedQty.RemainingQty < reqBondQty.Quantity)
                    errors.Add($"Remaining quantity ({remApprovedQty.RemainingQty}) of {remApprovedQty.ApprovalType.ToString("g")}-{remApprovedQty.ApprovalRef} is not sufficient to deliver {reqBondQty.Quantity}");
            }

            if (errors.Any())
            {
                throw new JCTOValidationException(string.Join(", ", errors));
            }
        }

        private async Task<List<EntryTransaction>> CreateNewEntryTxnsAsync(List<OrderStockReleaseEntryDto> releaseEntries, Order order)
        {
            var entryTxnsGrp = releaseEntries
                .GroupBy(e => e.EntryNo)
                .Select(g => new { EntryNo = g.Key, entries = g.ToList() });

            var orderReleaseTxns = new List<EntryTransaction>();
            foreach (var txns in entryTxnsGrp)
            {
                var entryTxns = await _entryService.CreateOrderEntryTransactionsAsync(txns.EntryNo, order, txns.entries);
                orderReleaseTxns.AddRange(entryTxns);
            }
            return orderReleaseTxns;
        }
    }
}
