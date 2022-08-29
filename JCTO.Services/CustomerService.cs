﻿using JCTO.Domain;
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
            var customerStocksRaw = await _dataContext.Customers
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
                            UndeliveredStock = g.SelectMany(en => en.Transactions)
                                .Where(t => t.Type == EntryTransactionType.Out
                                    && t.Order.Status == OrderStatus.Undelivered)
                                .Select(t => t.DeliveredQuantity * -1)
                                .ToArray(),
                            ProductId = g.Key,
                            RemainingStock = g.Select(en => en.RemainingQuantity).ToArray(),

                        }).ToList()
                }).ToListAsync();

            var customerStocks = customerStocksRaw.Select(cs => new CustomerStockDto
            {
                CustomerId = cs.CustomerId,
                CustomerName = cs.CustomerName,
                Stocks = cs.Stocks.Select(s => new ProductStockDto
                {
                    ProductId = s.ProductId,
                    RemainingStock = s.RemainingStock.Sum(),
                    UndeliveredStock = s.UndeliveredStock.Sum(),
                }).ToList()
            }).ToList();

            return customerStocks;
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
