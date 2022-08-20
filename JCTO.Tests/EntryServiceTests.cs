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
                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);
                  },
                  async (IDataContext dbContext) =>
                  {
                      var entrySvc = new EntryService(dbContext);

                      var customerId = await GetCustomerIdAsync(dbContext, "JVC");
                      var productId = await GetProductIdAsync(dbContext, "GO");

                      var entryDto = DtoHelper.CreateEntryDto("10001", customerId, productId, new DateTime(2022, 8, 20), EntryStatus.Active, 1000.1234);

                      var entry = await entrySvc.CreateAsync(entryDto);

                      Assert.NotNull(entry);
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

                      var customerId = await GetCustomerIdAsync(dbContext, "JVC");
                      var productId = await GetProductIdAsync(dbContext, "GO");

                      var entryDto = DtoHelper.CreateEntryDto("1001", customerId, productId, new DateTime(2022, 8, 20), EntryStatus.Active, 1000);

                      var ex = await Assert.ThrowsAsync<DbUpdateException>(() => entrySvc.CreateAsync(entryDto));
                  });
            }
        }

        private static async Task SetupTestDataAsync(IDataContext dbContext)
        {
            dbContext.Customers.AddRange(TestData.Customers.GetCustomers());
            dbContext.Products.AddRange(TestData.Products.GetProducts());
            await dbContext.SaveChangesAsync();

            var customerId = await GetCustomerIdAsync(dbContext, "JVC");
            var productId = await GetProductIdAsync(dbContext, "GO");

            dbContext.Entries.AddRange(TestData.Entries.GetEntries(customerId, productId));
            await dbContext.SaveChangesAsync();
        }

        private static async Task<Guid> GetCustomerIdAsync(IDataContext dataContext, string name)
        {
            return (await dataContext.Customers.FirstAsync(c => c.Name == name)).Id;
        }

        private static async Task<Guid> GetProductIdAsync(IDataContext dataContext, string code)
        {
            return (await dataContext.Products.FirstAsync(c => c.Code == code)).Id;
        }
    }
}