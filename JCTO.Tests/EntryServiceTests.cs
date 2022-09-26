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
                      var entrySvc = CreateService(dbContext);

                      var entryDto = DtoHelper.CreateEntryDto(customerId, productId, "10001", new DateTime(2022, 8, 20), EntryStatus.Active, 50.1234);

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
                      Assert.Equal(50.1234, newEntry.InitialQualtity);
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
                      var entrySvc = CreateService(dbContext);

                      var customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      var productId = await EntityHelper.GetProductIdAsync(dbContext, "GO");

                      var entryDto = DtoHelper.CreateEntryDto(customerId, productId, "1001", new DateTime(2022, 8, 20), EntryStatus.Active, 100);

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
                      var entrySvc = CreateService(dbContext);

                      var entries = await entrySvc.SearchEntriesAsync(new EntrySearchDto() { Page = 1, PageSize = 100 });

                      Assert.NotNull(entries);
                      Assert.True(entries.Total > 0);
                      Assert.Equal(entries.Total, entries.Items.Count);

                      var entry = entries.Items.First(o => o.EntryNo == "1002");

                      Assert.Equal(new DateTime(2022, 8, 28), entry.EntryDate);
                      Assert.Equal(500, entry.InitialQuantity);
                      Assert.Equal(220, entry.RemainingQuantity);
                      Assert.Equal("JVC", entry.Customer);
                      Assert.Equal("380_LSFO", entry.Product);
                      Assert.Equal(EntryStatus.Active, entry.Status);

                      Assert.Equal(3, entry.Transactions.Count);

                      var tr1 = entry.Transactions.First(t => t.ObRef == "ref-10");

                      Assert.Equal(-189.5, tr1.Quantity);
                      Assert.Equal(-180, tr1.DeliveredQuantity);
                      Assert.Equal(new DateTime(2022, 8, 27), tr1.TransactionDate);
                      Assert.Equal(1501, tr1.OrderNo);
                  });
            }
        }

        public class AddApproval
        {
            [Theory]
            [InlineData(ApprovalType.Rebond, null, "2022-8-31", 100, "Approval Ref. is required for Xbond and Rebond approvals")]
            [InlineData(ApprovalType.Rebond, "50010", "2022-8-31", 200, "Approving quantity (200) is greater than remaining quantity (150) to approve")]
            [InlineData(ApprovalType.Rebond, null, "2022-8-31", 200, "Approval Ref. is required for Xbond and Rebond approvals, Approving quantity (200) is greater than remaining quantity (150) to approve")]
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
                      var entrySvc = CreateService(dbContext);

                      var dto = DtoHelper.CreateEntryApprovalDto(entryId, approvalType, approvalRef, date, qty);

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => entrySvc.AddApprovalAsync(dto));

                      Assert.Equal(ex.Message, expectedError);
                  });
            }
        }

        public class GetEntryRemainingApprovals
        {
            [Theory]
            [InlineData("1001", "1:50000:1000.25")]
            [InlineData("1002", "2:60000:70")]
            [InlineData("1102", "1:50001:150")]
            public async Task WhenEntryExists_ReturnApprovalsWithBalances(string entryNo, string expected)
            {
                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);
                  },
                  async (IDataContext dbContext) =>
                  {
                      var entrySvc = CreateService(dbContext);

                      var approvals = await entrySvc.GetEntryRemainingApprovalsAsync(entryNo);

                      var expectedApprovals = expected.Split('|', StringSplitOptions.RemoveEmptyEntries);

                      Assert.Equal(expectedApprovals.Length, approvals.Count());

                      foreach (var expectedApproval in expectedApprovals)
                      {
                          var approvalData = expectedApproval.Split(":", StringSplitOptions.RemoveEmptyEntries);
                          var aprType = Enum.Parse(typeof(ApprovalType), approvalData[0]);
                          var aprRef = approvalData[1];
                          var remAmount = double.Parse(approvalData[2]);

                          var actual = approvals.Find(a => a.ApprovalRef == aprRef);

                          Assert.Equal(entryNo, actual.EntryNo);
                          Assert.Equal(remAmount, actual.RemainingQty);
                          Assert.Equal(aprType, actual.ApprovalType);
                      }
                  });
            }
        }

        public class Update
        {
            [Theory]
            [InlineData("1001")]
            [InlineData("1002")]
            [InlineData("1103")]
            public async Task WhenModifingQtyWhenHavingTransactions_ThrowsException(string entryNo)
            {
                var entryId = Guid.Empty;
                var customerId = Guid.Empty;
                var productId = Guid.Empty;
                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);
                      entryId = await EntityHelper.GetEntryIdAsync(dbContext, entryNo);

                  },
                  async (IDataContext dbContext) =>
                  {
                      var entrySvc = CreateService(dbContext);

                      customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      productId = await EntityHelper.GetProductIdAsync(dbContext, "GO");

                      var entryDto = DtoHelper.CreateEntryDto(customerId, productId, entryNo, new DateTime(2022, 9, 19), EntryStatus.Active, 2000);

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => entrySvc.UpdateAsync(entryId, entryDto));

                      Assert.Equal("Can't update the quantity of an entry where there approvals and/or order releases", ex.Message);
                  });
            }

            [Fact]
            public async Task WhenNotModifingQty_UpdateSuccessfully()
            {
                var entryNo = "1002";
                var entryId = Guid.Empty;
                var customerId = Guid.Empty;
                var productId = Guid.Empty;
                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);
                      entryId = await EntityHelper.GetEntryIdAsync(dbContext, entryNo);
                      customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      productId = await EntityHelper.GetProductIdAsync(dbContext, "GO");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var entrySvc = CreateService(dbContext);

                      var entry = await dbContext.Entries.FindAsync(entryId);

                      var entryDto = DtoHelper.CreateEntryDto(customerId, productId, "10021", new DateTime(2022, 9, 19), EntryStatus.Active, 500);
                      entryDto.ConcurrencyKey = entry.ConcurrencyKey;

                      var result = await entrySvc.UpdateAsync(entryId, entryDto);

                      Assert.NotNull(result);
                  },
                  async (IDataContext dbContext) =>
                  {
                      var entry = await dbContext.Entries.FindAsync(entryId);

                      Assert.Equal("10021", entry.EntryNo);
                      Assert.Equal(new DateTime(2022, 9, 19), entry.EntryDate);
                      Assert.Equal(500, entry.InitialQualtity);
                      Assert.Equal(220, entry.RemainingQuantity);
                  });
            }

            [Fact]
            public async Task WhenNoTxnsAndIncreasingQty_UpdateSuccessfully()
            {
                var entryNo = "1103";
                var entryId = Guid.Empty;
                var customerId = Guid.Empty;
                var productId = Guid.Empty;
                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);
                      entryId = await EntityHelper.GetEntryIdAsync(dbContext, entryNo);
                      customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      productId = await EntityHelper.GetProductIdAsync(dbContext, "GO");

                      var txns = await dbContext.EntryTransactions.Where(t => t.EntryId == entryId).ToListAsync();
                      dbContext.EntryTransactions.RemoveRange(txns);
                      await dbContext.SaveChangesAsync();
                  },
                  async (IDataContext dbContext) =>
                  {
                      var entrySvc = CreateService(dbContext);

                      var entry = await dbContext.Entries.FindAsync(entryId);

                      var entryDto = DtoHelper.CreateEntryDto(customerId, productId, "1103", new DateTime(2022, 9, 19), EntryStatus.Active, 250);
                      entryDto.ConcurrencyKey = entry.ConcurrencyKey;

                      var result = await entrySvc.UpdateAsync(entryId, entryDto);

                      Assert.NotNull(result);
                  },
                  async (IDataContext dbContext) =>
                  {
                      var entry = await dbContext.Entries.Where(e => e.Id == entryId)
                        .FirstAsync();

                      Assert.Equal("1103", entry.EntryNo);
                      Assert.Equal(new DateTime(2022, 9, 19), entry.EntryDate);
                      Assert.Equal(250, entry.InitialQualtity);
                      Assert.Equal(250, entry.RemainingQuantity);
                  });
            }

            [Fact]
            public async Task WhenNoTxnsAndDecreasingQty_UpdateSuccessfully()
            {
                var entryNo = "1103";
                var entryId = Guid.Empty;
                var customerId = Guid.Empty;
                var productId = Guid.Empty;
                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);
                      entryId = await EntityHelper.GetEntryIdAsync(dbContext, entryNo);
                      customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      productId = await EntityHelper.GetProductIdAsync(dbContext, "GO");

                      var txns = await dbContext.EntryTransactions.Where(t => t.EntryId == entryId).ToListAsync();
                      dbContext.EntryTransactions.RemoveRange(txns);
                      await dbContext.SaveChangesAsync();
                  },
                  async (IDataContext dbContext) =>
                  {
                      var entrySvc = CreateService(dbContext);

                      var entry = await dbContext.Entries.FindAsync(entryId);

                      var entryDto = DtoHelper.CreateEntryDto(customerId, productId, "1103", new DateTime(2022, 9, 19), EntryStatus.Active, 150);
                      entryDto.ConcurrencyKey = entry.ConcurrencyKey;

                      var result = await entrySvc.UpdateAsync(entryId, entryDto);

                      Assert.NotNull(result);
                  },
                  async (IDataContext dbContext) =>
                  {
                      var entry = await dbContext.Entries.Where(e => e.Id == entryId)
                        .FirstAsync();

                      Assert.Equal("1103", entry.EntryNo);
                      Assert.Equal(new DateTime(2022, 9, 19), entry.EntryDate);
                      Assert.Equal(150, entry.InitialQualtity);
                      Assert.Equal(150, entry.RemainingQuantity);
                  });
            }
        }

        public class Delete
        {
            [Fact]
            public async Task WhenHavingTransactions_ThrowsException()
            {
                string entryNo = "1001";
                var entryId = Guid.Empty;
                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);
                      entryId = await EntityHelper.GetEntryIdAsync(dbContext, entryNo);
                  },
                  async (IDataContext dbContext) =>
                  {
                      var entrySvc = CreateService(dbContext);

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => entrySvc.DeleteAsync(entryId));

                      Assert.Equal("Can't delete an entry when there are approvals and/or order releases", ex.Message);
                  });
            }

            [Fact]
            public async Task WhenNotHavingTransactions_DeleteSuccessfully()
            {
                string entryNo = "1103";
                var entryId = Guid.Empty;
                var customerId = Guid.Empty;
                var productId = Guid.Empty;
                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);
                      entryId = await EntityHelper.GetEntryIdAsync(dbContext, entryNo);

                      customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      productId = await EntityHelper.GetProductIdAsync(dbContext, "380_LSFO");

                      var txns = await dbContext.EntryTransactions.Where(t => t.EntryId == entryId).ToListAsync();
                      dbContext.EntryTransactions.RemoveRange(txns);
                      await dbContext.SaveChangesAsync();
                  },
                  async (IDataContext dbContext) =>
                  {
                      var entrySvc = CreateService(dbContext);

                      await entrySvc.DeleteAsync(entryId);
                  },
                  async (IDataContext dbContext) =>
                  {
                      var entry = await dbContext.Entries.FindAsync(entryId);

                      Assert.Null(entry);
                  });
            }
        }

        private static async Task SetupTestDataAsync(IDataContext dbContext)
        {
            await TestData.Orders.SetupOrderAndEntryTestDataAsync(dbContext);
        }

        private static EntryService CreateService(IDataContext dbContext)
        {
            var entrySvc = new EntryService(dbContext);

            return entrySvc;
        }
    }
}
