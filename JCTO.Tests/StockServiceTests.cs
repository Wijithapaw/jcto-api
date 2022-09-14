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
                      lsfo_productId = await EntityHelper.GetProductIdAsync(dbContext, "380_HSFO");
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

                      Assert.Equal(1155, dischargeTxn.Stock.RemainingQuantity);
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

                      var stockTxn = await stockSvc.DebitForEntryAsync("501", 25.250, new DateTime(2022, 9, 9));

                      Assert.NotNull(stockTxn);

                      Assert.Equal(1054.75, stockTxn.Stock.RemainingQuantity);

                      Assert.Equal(StockTransactionType.Out, stockTxn.Type);
                      Assert.Equal(-25.250, stockTxn.Quantity);
                      Assert.Equal(new DateTime(2022, 9, 9), stockTxn.TransactionDate);
                  });
            }

            [Fact]
            public async Task WhenToBondNoNotExists_ThrowsException()
            {
                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);
                  },
                  async (IDataContext dbContext) =>
                  {
                      var stockSvc = new StockService(dbContext);

                      var entryId = Guid.NewGuid();

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => stockSvc.DebitForEntryAsync("5011", 25.250, new DateTime(2022, 9, 9)));

                      Assert.Equal("No discharge available for ToBondNo: 5011", ex.Message);
                  });
            }

            [Fact]
            public async Task WhenStockIsNotSufficient_ThrowsException()
            {
                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);
                  },
                  async (IDataContext dbContext) =>
                  {
                      var stockSvc = new StockService(dbContext);

                      var entryId = Guid.NewGuid();

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => stockSvc.DebitForEntryAsync("502", 125.25, new DateTime(2022, 9, 9)));

                      Assert.Equal($"Remaining quantity(50) in To Bond No: 502 is not sufficient to create an entry having quantity: 125.25", ex.Message);
                  });
            }
        }

        public class SearchStockDischarges
        {
            [Theory]
            [InlineData("", "", null, null, 9)]
            [InlineData("JVC", "", null, null, 4)]
            [InlineData("", "GO", null, null, 7)]
            [InlineData("", "380_LSFO", null, null, 2)]
            [InlineData("Mobil", "", null, null, 0)]
            [InlineData("", "380_HSFO", null, null, 0)]
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
            [InlineData("", "", "2022-9-1", "2022-9-2", 6)]
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
