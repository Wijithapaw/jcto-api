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
                ObRefPrefix = dto.ObRefPrefix,
                TankNo = dto.TankNo,
                BuyerType = dto.BuyerType,
                Remarks = dto.Remarks
            };

            _dataContext.Orders.Add(order);

            var entryTxnsGrp = dto.ReleaseEntries
                .GroupBy(e => e.EntryNo)
                .Select(g => new { EntryNo = g.Key, entries = g.ToList() });

            var orderReleaseTxns = new List<EntryTransaction>();
            foreach (var txns in entryTxnsGrp)
            {
                var entryTxns = await _entryService.CreateOrderEntryTransactionsAsync(txns.EntryNo, txns.entries);
                orderReleaseTxns.AddRange(entryTxns);
            }

            order.Transactions = orderReleaseTxns;

            order.BowserEntries = dto.BowserEntries.Select(b => new BowserEntry
            {
                Capacity = b.Capacity,
                Count = b.Count,
            }).ToList();

            await _dataContext.SaveChangesAsync();

            return GetEntityCreateResult(order);
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
                    Status = o.Status,
                    TankNo = o.TankNo,
                    Remarks = o.Remarks,
                    ReleaseEntries = o.Transactions.Select(t => new OrderStockReleaseEntryDto
                    {
                        Id = t.Id,
                        EntryNo = t.Entry.EntryNo,
                        ObRef = t.ObRef,
                        DeliveredQuantity = -1 * t.DeliveredQuantity,
                        Quantity = -1 * t.Quantity
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
            filter.OrderNo = filter.OrderNo?.ToLower().Trim() ?? "";

            var orders = await _dataContext.Orders
                .Where(o => (filter.CustomerId == null || filter.CustomerId == o.CustomerId)
                    && (filter.ProductId == null || filter.ProductId == o.ProductId)
                    && (filter.From == null || o.OrderDate >= filter.From)
                    && (filter.To == null || o.OrderDate <= filter.To)
                    && (filter.Status == null || o.Status == filter.Status)
                    && (filter.BuyerType == null || o.BuyerType == filter.BuyerType)
                    && (filter.Buyer == "" || o.Buyer.ToLower().Contains(filter.Buyer))
                    && (filter.OrderNo == "" || o.OrderNo.ToLower().Contains(filter.OrderNo)))
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
                    Status = o.Status
                }).GetPagedListAsync(filter);

            return orders;
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

            if (string.IsNullOrWhiteSpace(order.OrderNo))
                errors.Add("Order No. not found");

            if (string.IsNullOrWhiteSpace(order.Buyer))
                errors.Add("Buyer not found");

            if (order.Quantity <= 0)
                errors.Add("Quantity must be > 0");

            if (string.IsNullOrWhiteSpace(order.ObRefPrefix))
                errors.Add("OBRef not found");

            if (string.IsNullOrWhiteSpace(order.TankNo))
                errors.Add("Tank No. not found");

            if (!order.ReleaseEntries.Any())
            {
                errors.Add("Stock release entries not found");
            }
            else
            {
                if (order.ReleaseEntries.Sum(e => e.DeliveredQuantity) != order.Quantity)
                    errors.Add("Sum of Delivered Quantities not equal to overall Quantity");

                if (order.ReleaseEntries.Any(e => e.DeliveredQuantity > e.Quantity))
                    errors.Add("Stock release entries found having Delevered Quantity > Quantity");
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

        private async Task ValidateEntriesAsync(OrderDto order)
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
                    e.RemainingQuantity,
                    e.InitialQualtity,
                    e.Status
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

            if (completedEntries.Any())
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

            var reqQtysFromEntries = order.ReleaseEntries
                .Where(re => !invalidEntries.Union(completedEntries).Contains(re.EntryNo))
                .GroupBy(e => e.EntryNo)
                .Select(e => new
                {
                    EntryNo = e.Key,
                    Quantity = e.Sum(r => r.DeliveredQuantity)
                }).ToArray();

            foreach (var reqQty in reqQtysFromEntries)
            {
                var entry = entries.First(e => e.EntryNo == reqQty.EntryNo);
                if (entry.RemainingQuantity < reqQty.Quantity)
                    errors.Add($"Remaining quantity: {entry.RemainingQuantity} of Entry: {reqQty.EntryNo} not sufficient to deliver requested quantity: {reqQty.Quantity}");
            }

            if (errors.Any())
            {
                throw new JCTOValidationException(string.Join(", ", errors));
            }
        }
    }
}
