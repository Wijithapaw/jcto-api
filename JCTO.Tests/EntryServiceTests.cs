﻿using JCTO.Domain;
using JCTO.Domain.Enums;
using JCTO.Services;
using JCTO.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace JCTO.Tests
{
    public class EntryServiceTests
    {
        public class Create
        {
            [Fact]
            public async Task WhenPassingValidData_CreateSuccessfully()
            {
                Guid id = Guid.Empty;
                var customerId = Guid.Empty;
                var productId = Guid.Empty;

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      productId = await EntityHelper.GetProductIdAsync(dbContext, "GO");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var entrySvc = new EntryService(dbContext);

                      var entryDto = DtoHelper.CreateEntryDto("10001", customerId, productId, new DateTime(2022, 8, 20), EntryStatus.Active, 1000.1234);

                      var entry = await entrySvc.CreateAsync(entryDto);

                      Assert.NotNull(entry);

                      id = entry.Id;
                  },
                  async (IDataContext dbContext) =>
                  {
                      var newEntry = await dbContext.Entries
                        .Where(e => e.Id == id)
                        .Include(e => e.Transactions)
                        .FirstAsync();

                      Assert.True(newEntry != null);
                      Assert.Equal(id, newEntry.Id);
                      Assert.Equal(customerId, newEntry.CustomerId);
                      Assert.Equal(productId, newEntry.ProductId);
                      Assert.Equal(1000.1234, newEntry.InitialQualtity);
                      Assert.Equal("10001", newEntry.EntryNo);
                      Assert.Equal(new DateTime(2022, 8, 20), newEntry.EntryDate);
                      Assert.Equal(EntryStatus.Active, newEntry.Status);

                      Assert.Single(newEntry.Transactions);

                      var txn = newEntry.Transactions.First();

                      Assert.Equal(1000.1234, txn.Quantity);
                      Assert.Equal(id, txn.EntryId);
                      Assert.NotEqual(DateTime.MinValue, txn.TransactionDateTimeUtc);
                  });
            }

            [Fact]
            public async Task WhenPassingExistingEntryNo_ThrowsException()
            {
                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);
                  },
                  async (IDataContext dbContext) =>
                  {
                      var entrySvc = new EntryService(dbContext);

                      var customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      var productId = await EntityHelper.GetProductIdAsync(dbContext, "GO");

                      var entryDto = DtoHelper.CreateEntryDto("1001", customerId, productId, new DateTime(2022, 8, 20), EntryStatus.Active, 1000);

                      var ex = await Assert.ThrowsAsync<DbUpdateException>(() => entrySvc.CreateAsync(entryDto));
                  });
            }
        }

        private static async Task SetupTestDataAsync(IDataContext dbContext)
        {
            await TestData.Orders.SetupOrderAndEntryTestDataAsync(dbContext);
        }
    }
}
