using JCTO.Domain;
using JCTO.Domain.CustomExceptions;
using JCTO.Domain.Dtos;
using JCTO.Domain.Enums;
using JCTO.Services;
using JCTO.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCTO.Tests
{
    public class StockServiceTests
    {
        public class Topup
        {
            [Fact]
            public async Task WhenStockNotExists_CreateOneSuccessfully()
            {
                var id = Guid.Empty;
                var jvc_customerId = Guid.Empty;
                var lsfo_productId = Guid.Empty;

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      jvc_customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      lsfo_productId = await EntityHelper.GetProductIdAsync(dbContext, "380_LSFO");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var stockSvc = new StockService(dbContext);

                      var dto = new StockTopupDto
                      {
                          CustomerId = jvc_customerId,
                          ProductId = lsfo_productId,
                          Quantity = 100,
                          ToBondNo = "1000",
                          TransactionDate = new DateTime(2022, 9, 8)
                      };

                      var result = await stockSvc.TopupAsync(dto);

                      id = result.Id;
                  },
                  async (IDataContext dbContext) =>
                  {
                      var dischargeTxn = await dbContext.StockTransactions
                        .Include(t => t.Stock)
                        .FirstOrDefaultAsync(t => t.Id == id);

                      Assert.NotNull(dischargeTxn);
                      Assert.NotNull(dischargeTxn.Stock);

                      Assert.Equal(100, dischargeTxn.Stock.RemainingQuantity);
                      Assert.Equal(jvc_customerId, dischargeTxn.Stock.CustomerId);
                      Assert.Equal(lsfo_productId, dischargeTxn.Stock.ProductId);

                      Assert.Equal("1000", dischargeTxn.ToBondNo);
                      Assert.Equal(new DateTime(2022, 9, 8), dischargeTxn.TransactionDate);
                  });
            }

            [Fact]
            public async Task WhenStockExists_UpdateRemainingQtySuccessfully()
            {
                var id = Guid.Empty;
                var jvc_customerId = Guid.Empty;
                var go_productId = Guid.Empty;

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      jvc_customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      go_productId = await EntityHelper.GetProductIdAsync(dbContext, "GO");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var stockSvc = new StockService(dbContext);

                      var dto = new StockTopupDto
                      {
                          CustomerId = jvc_customerId,
                          ProductId = go_productId,
                          Quantity = 75,
                          ToBondNo = "1000",
                          TransactionDate = new DateTime(2022, 9, 9)
                      };

                      var result = await stockSvc.TopupAsync(dto);

                      id = result.Id;
                  },
                  async (IDataContext dbContext) =>
                  {
                      var dischargeTxn = await dbContext.StockTransactions
                        .Include(t => t.Stock)
                        .FirstOrDefaultAsync(t => t.Id == id);

                      Assert.NotNull(dischargeTxn);
                      Assert.NotNull(dischargeTxn.Stock);

                      Assert.Equal(175, dischargeTxn.Stock.RemainingQuantity);
                      Assert.Equal(jvc_customerId, dischargeTxn.Stock.CustomerId);
                      Assert.Equal(go_productId, dischargeTxn.Stock.ProductId);

                      Assert.Equal("1000", dischargeTxn.ToBondNo);
                      Assert.Equal(new DateTime(2022, 9, 9), dischargeTxn.TransactionDate);
                  });
            }
        }

        public class DebitForEntry
        {
            [Fact]
            public async Task WhenStockExists_CreatesDebitTxn()
            {
                var jvc_customerId = Guid.Empty;
                var go_productId = Guid.Empty;

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      jvc_customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      go_productId = await EntityHelper.GetProductIdAsync(dbContext, "GO");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var stockSvc = new StockService(dbContext);

                      await stockSvc.DebitForEntryAsync(jvc_customerId, go_productId, null, 25.250, new DateTime(2022, 9, 9));

                      var stock = dbContext.Stocks
                          .Where(s => s.CustomerId == jvc_customerId && s.ProductId == go_productId)
                          .Include(s => s.Transactions)
                          .First();

                      Assert.NotNull(stock);

                      Assert.Equal(74.750, stock.RemainingQuantity);

                      var txn = stock.Transactions.First();

                      Assert.Equal(StockTransactionType.Out, txn.Type);
                      Assert.Equal(-25.250, txn.Quantity);
                      Assert.Equal(new DateTime(2022, 9, 9), txn.TransactionDate);
                  });
            }

            [Fact]
            public async Task WhenStockDoesNotExists_ThrowsException()
            {
                var jvc_customerId = Guid.Empty;
                var lfso_productId = Guid.Empty;

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      jvc_customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      lfso_productId = await EntityHelper.GetProductIdAsync(dbContext, "380_LSFO");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var stockSvc = new StockService(dbContext);

                      var entryId = Guid.NewGuid();

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => stockSvc.DebitForEntryAsync(jvc_customerId, lfso_productId, null, 25.250, new DateTime(2022, 9, 9)));

                      Assert.Equal("No stock available for this customer and product", ex.Message);
                  });
            }

            [Fact]
            public async Task WhenStockIsNotSufficient_ThrowsException()
            {
                var jvc_customerId = Guid.Empty;
                var go_productId = Guid.Empty;

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      jvc_customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      go_productId = await EntityHelper.GetProductIdAsync(dbContext, "GO");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var stockSvc = new StockService(dbContext);

                      var entryId = Guid.NewGuid();

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => stockSvc.DebitForEntryAsync(jvc_customerId, go_productId, null, 125.250, new DateTime(2022, 9, 9)));

                      Assert.Equal($"Remaining quantity in stocks ({100}) not sufficient to create an entity with quantity: {125.25}", ex.Message);
                  });
            }
        }

        public class SearchStockDischarges
        {
            [Theory]
            [InlineData("", "", null, null, 7)]
            [InlineData("JVC", "", null, null, 3)]
            [InlineData("", "GO", null, null, 7)]
            [InlineData("", "380_LSFO", null, null, 0)]
            [InlineData("Mobil", "", null, null, 0)]
            public async Task WhenPassingFilter_ReturnCorrectRecordCount(string customerCode, string productCode, DateTime? from, DateTime? to, int expectedRecordCount)
            {
                Guid? customerId = null;
                Guid? productId = null;

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      if (!string.IsNullOrEmpty(customerCode))
                          customerId = await EntityHelper.GetCustomerIdAsync(dbContext, customerCode);

                      if (!string.IsNullOrEmpty(productCode))
                          productId = await EntityHelper.GetProductIdAsync(dbContext, productCode);
                  },
                  async (IDataContext dbContext) =>
                  {
                      var stockSvc = new StockService(dbContext);

                      var dto = new StockDischargeSearchDto
                      {
                          CustomerId = customerId,
                          ProductId = productId,
                          From = from == DateTime.MinValue ? null : from,
                          To = to == DateTime.MinValue ? null : to,
                          Page = 1,
                          PageSize = 100
                      };

                      var data = await stockSvc.SearchStockDischargesAsync(dto);

                      Assert.NotNull(data);

                      Assert.Equal(expectedRecordCount, data.Total);
                      Assert.Equal(expectedRecordCount, data.Items.Count);
                  });
            }

            [Theory]
            [InlineData("", "", "2022-9-1", "2022-9-2", 4)]
            [InlineData("JVC", "GO", "2022-9-1", "2022-9-2", 2)]
            public async Task WhenPassingFilter_ReturnCorrectRecordCount2(string customerCode, string productCode, DateTime from, DateTime to, int expectedRecordCount)
            {
                Guid? customerId = null;
                Guid? productId = null;

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      if (!string.IsNullOrEmpty(customerCode))
                          customerId = await EntityHelper.GetCustomerIdAsync(dbContext, customerCode);

                      if (!string.IsNullOrEmpty(productCode))
                          productId = await EntityHelper.GetProductIdAsync(dbContext, productCode);
                  },
                  async (IDataContext dbContext) =>
                  {
                      var stockSvc = new StockService(dbContext);

                      var dto = new StockDischargeSearchDto
                      {
                          CustomerId = customerId,
                          ProductId = productId,
                          From = from == DateTime.MinValue ? null : from,
                          To = to == DateTime.MinValue ? null : to,
                          Page = 1,
                          PageSize = 100
                      };

                      var data = await stockSvc.SearchStockDischargesAsync(dto);

                      Assert.NotNull(data);

                      Assert.Equal(expectedRecordCount, data.Total);
                      Assert.Equal(expectedRecordCount, data.Items.Count);
                  });
            }
        }

        private static async Task SetupTestDataAsync(IDataContext dbContext)
        {
            await TestData.Stocks.CreateStockAsync(dbContext);
        }
    }
}
