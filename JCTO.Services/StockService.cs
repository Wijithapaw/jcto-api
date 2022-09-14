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
    public class StockService : BaseService, IStockService
    {
        private readonly IDataContext _dataContext;

        public StockService(IDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<PagedResultsDto<StockDischargeListItemDto>> SearchStockDischargesAsync(StockDischargeSearchDto filter)
        {
            var discharges = await _dataContext.StockTransactions
                .Where(d => d.Type == StockTransactionType.In
                    && (filter.CustomerId == null || filter.CustomerId == d.Stock.CustomerId)
                    && (filter.ProductId == null || filter.ProductId == d.Stock.ProductId)
                    && (filter.From == null || d.TransactionDate >= filter.From)
                    && (filter.To == null || d.TransactionDate <= filter.To))
                .OrderByDescending(d => d.TransactionDate)
                .ThenByDescending(d => d.LastUpdatedDateUtc)
                .Select(d => new StockDischargeListItemDto
                {
                    Id = d.Id,
                    Date = d.TransactionDate,
                    ToBondNo = d.ToBondNo,
                    Customer = d.Stock.Customer.Name,
                    Product = d.Stock.Product.Code,
                    Quantity = d.Quantity
                }).GetPagedListAsync(filter);

            return discharges;
        }

        public async Task<StockTransaction> DebitForEntryAsync(string toBondNo, double quantity, DateTime date)
        {
            var dischargeTxn = await _dataContext.StockTransactions
                .Include(t => t.Stock)
                .Where(t => t.Type == StockTransactionType.In
                    && t.ToBondNo.ToLower() == toBondNo.ToLower().Trim())
                .FirstOrDefaultAsync();

            if (dischargeTxn == null)
                throw new JCTOValidationException($"No discharge available for ToBondNo: {toBondNo}");

            var remainingQty = await _dataContext.StockTransactions
                .Where(t => t.Id == dischargeTxn.Id || t.DischargeTransactionId == dischargeTxn.Id)
                .SumAsync(t => t.Quantity);

            if (remainingQty < quantity)
                throw new JCTOValidationException($"Remaining quantity({remainingQty}) in To Bond No: {toBondNo} is not sufficient to create an entry having quantity: {quantity}");

            var stockTxn = new StockTransaction
            {
                Stock = dischargeTxn.Stock,
                DischargeTransaction = dischargeTxn,
                Quantity = -1 * Math.Abs(quantity),
                TransactionDate = date,
                Type = StockTransactionType.Out,
            };

            dischargeTxn.Stock.RemainingQuantity += stockTxn.Quantity;

            return stockTxn;
        }

        public async Task<EntityCreateResult> TopupAsync(StockTopupDto dto)
        {
            var stock = await GetOrCreateStockAsync(dto.CustomerId, dto.ProductId);

            var stockTxn = new StockTransaction
            {
                Stock = stock,
                Quantity = Math.Abs(dto.Quantity),
                ToBondNo = dto.ToBondNo,
                TransactionDate = dto.TransactionDate,
                Type = StockTransactionType.In,
            };

            stock.RemainingQuantity += stockTxn.Quantity;

            _dataContext.StockTransactions.Add(stockTxn);

            await _dataContext.SaveChangesAsync();

            return GetEntityCreateResult(stockTxn);
        }

        private async Task<Stock> GetOrCreateStockAsync(Guid customerId, Guid productId)
        {
            var stock = await _dataContext.Stocks
                .Where(s => s.CustomerId == customerId && s.ProductId == productId)
                .FirstOrDefaultAsync();

            if (stock == null)
            {
                var newStock = new Stock
                {
                    CustomerId = customerId,
                    ProductId = productId,
                    RemainingQuantity = 0
                };
                stock = newStock;
            }

            return stock;
        }
    }
}
