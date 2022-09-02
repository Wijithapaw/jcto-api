using JCTO.Domain;
using JCTO.Domain.CustomExceptions;
using JCTO.Domain.Dtos;
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

        public class SearchEntries
        {
            [Fact]
            public async Task WhenNoFilters_ReturnAll()
            {
                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);
                  },
                  async (IDataContext dbContext) =>
                  {
                      var entrySvc = new EntryService(dbContext);

                      var entries = await entrySvc.SearchEntriesAsync(new EntrySearchDto() { Page = 1, PageSize = 100 });

                      Assert.NotNull(entries);
                      Assert.True(entries.Total > 0);
                      Assert.Equal(entries.Total, entries.Items.Count);

                      var entry = entries.Items.First(o => o.EntryNo == "1002");

                      Assert.Equal(new DateTime(2022, 8, 28), entry.EntryDate);
                      Assert.Equal(500, entry.InitialQuantity);
                      Assert.Equal(205.5, entry.RemainingQuantity);
                      Assert.Equal("JVC", entry.Customer);
                      Assert.Equal("380_LSFO", entry.Product);
                      Assert.Equal(EntryStatus.Active, entry.Status);

                      Assert.Equal(2, entry.Transactions.Count);

                      var tr1 = entry.Transactions.First(t => t.ObRef == "ref-10");

                      Assert.Equal(200.250, tr1.Quantity);
                      Assert.Equal(199.500, tr1.DeliveredQuantity);
                      Assert.Equal(new DateTime(2022, 8, 27), tr1.OrderDate);
                      Assert.Equal("1501", tr1.OrderNo);
                  });
            }
        }

        public class AddApproval
        {
            [Theory]
            [InlineData(ApprovalType.Rebond, null, "2022-8-31", 100, "Approval Ref. is required for Xbond and Rebond approvals")]
            [InlineData(ApprovalType.Rebond, "50010", "2022-8-31", 200, "Approving quantity is greater than remaining amount to approve")]
            [InlineData(ApprovalType.Rebond, null, "2022-8-31", 200, "Approval Ref. is required for Xbond and Rebond approvals, Approving quantity is greater than remaining amount to approve")]
            public async Task WhenNoFilters_ReturnAll(ApprovalType approvalType, string approvalRef, DateTime date, double qty, string expectedError)
            {
                var entryId = Guid.Empty;

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);
                      entryId = await EntityHelper.GetEntryIdAsync(dbContext, "1002");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var entrySvc = new EntryService(dbContext);

                      var dto = DtoHelper.CreateEntryApprovalDto(entryId, approvalType, approvalRef, date, qty);

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => entrySvc.AddApprovalAsync(dto));

                      Assert.Equal(ex.Message, expectedError);
                  });
            }
        }

        private static async Task SetupTestDataAsync(IDataContext dbContext)
        {
            await TestData.Orders.SetupOrderAndEntryTestDataAsync(dbContext);
        }
    }
}
