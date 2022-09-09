using JCTO.Domain;
using JCTO.Domain.Dtos;
using JCTO.Domain.Enums;
using JCTO.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace JCTO.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IDataContext _dataContext;

        public CustomerService(IDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<List<ListItem>> GetAllCustomersListItemsAsync()
        {
            var listItems = await _dataContext.Customers
               .Where(c => !c.Inactive)
               .Select(c => new ListItem { Id = c.Id.ToString(), Label = c.Name })
               .ToListAsync();

            return listItems;
        }

        public async Task<List<CustomerStockDto>> GetAllCustomerStocksAsync()
        {
            var stockBalance = await _dataContext.Customers
                .Where(c => !c.Inactive)
                .Select(c => new CustomerStockDto
                {
                    CustomerId = c.Id,
                    CustomerName = c.Name,
                    Stocks = c.Stocks.Select(p => new ProductStockDto
                    {
                        ProductId = p.ProductId,
                        RemainingStock = p.RemainingQuantity,
                        UndeliveredStock = 0
                    }).ToList()
                }).ToListAsync();

            var entryBalance = await _dataContext.Customers
                .Where(c => !c.Inactive)
                .Select(c => new
                {
                    CustomerId = c.Id,
                    CustomerName = c.Name,
                    Stocks = c.Entries
                        .Where(e => e.Status == EntryStatus.Active)
                        .GroupBy(e => e.ProductId)
                        .Select(g => new
                        {
                            UndeliveredStocks = g.SelectMany(en => en.Transactions)
                                .Where(t => t.Type == EntryTransactionType.Out
                                    && t.Order.Status == OrderStatus.Undelivered)
                                .Select(t => t.Quantity * -1)
                                .ToArray(),
                            ProductId = g.Key,
                            RemainingStocks = g.Select(en => en.RemainingQuantity).ToArray(),
                        }).ToList()
                }).Select(cs => new CustomerStockDto
                {
                    CustomerId = cs.CustomerId,
                    CustomerName = cs.CustomerName,
                    Stocks = cs.Stocks.Select(s => new ProductStockDto
                    {
                        ProductId = s.ProductId,
                        RemainingStock = s.RemainingStocks.Sum(),
                        UndeliveredStock = s.UndeliveredStocks.Sum()
                    }).ToList()
                }).ToListAsync();

            foreach (var cusStockBal in stockBalance)
            {
                foreach (var cusProdBal in cusStockBal.Stocks)
                {
                    var cusProdEntryBal = entryBalance
                        .FirstOrDefault(e => e.CustomerId == cusStockBal.CustomerId)?
                        .Stocks.FirstOrDefault(s => s.ProductId == cusProdBal.ProductId);

                    if (cusProdEntryBal != null)
                    {
                        cusProdBal.RemainingStock += cusProdEntryBal.RemainingStock;
                        cusProdBal.UndeliveredStock += cusProdEntryBal.UndeliveredStock;
                    }
                }
            }

            return stockBalance;
        }

        public async Task<List<ListItem>> GetProductListItemsAsync()
        {
            var products = await _dataContext.Products
                .Where(p => !p.Inactive)
                .OrderBy(p => p.SortOrder)
                .Select(p => new ListItem
                {
                    Id = p.Id.ToString(),
                    Label = p.Code
                }).ToListAsync();

            return products;
        }
    }
}
