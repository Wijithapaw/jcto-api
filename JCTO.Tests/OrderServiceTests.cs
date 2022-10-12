using JCTO.Domain;
using JCTO.Domain.ConfigSettings;
using JCTO.Domain.CustomExceptions;
using JCTO.Domain.Dtos;
using JCTO.Domain.Entities;
using JCTO.Domain.Enums;
using JCTO.Services;
using JCTO.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace JCTO.Tests
{
    public class OrderServiceTests
    {
        public class Create
        {
            [Theory]
            [InlineData(1001, "2022-8-26", 111, "Dialog", OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge, null, "Sum of release Quantities not equal to overall Quantity")]
            [InlineData(1001, null, 110, "Dialog", OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge, null, "Order Date not found")]
            [InlineData(0, "2022-8-26", 110, "Dialog", OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge, null, "Order No. not valid")]
            [InlineData(1001, "2022-8-26", 110, "", OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge, null, "Buyer not found")]
            [InlineData(1001, "2022-8-26", 110, "Dialog", OrderStatus.Undelivered, "", "", BuyerType.Barge, null, "OBRef not found, Tank No. not found")]
            [InlineData(1001, "2022-8-26", 0, "Dialog", OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge, null, "Quantity must be > 0, Sum of release Quantities not equal to overall Quantity")]
            [InlineData(1001, "2022-8-26", 0, "", OrderStatus.Undelivered, "", "", BuyerType.Barge, null, "Buyer not found, Quantity must be > 0, OBRef not found, Tank No. not found, Sum of release Quantities not equal to overall Quantity")]
            public async Task WhenPassingValidData_CreateSuccessfully(int orderNo, DateTime orderDate, decimal quantity,
            string buyer, OrderStatus status, string obPrefix, string tankNo, BuyerType buyerType,
            string remarks, string expectedError)
            {
                var customerId = Guid.Empty;
                var productId = Guid.Empty;
                List<OrderStockReleaseEntryDto> releaseEntries = null;

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      productId = await EntityHelper.GetProductIdAsync(dbContext, "GO");
                      var approvalId = await EntityHelper.GetGetEntryTxnIdAsync(dbContext, "1001", ApprovalType.Rebond, "50000");

                      releaseEntries = new List<OrderStockReleaseEntryDto>
                      {
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), ApprovalId=approvalId, EntryNo="1001", ObRef="xyz", Quantity = 110, DeliveredQuantity=0 }
                      };
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, customerId, productId, orderNo, orderDate, quantity, null,
                          buyer, status, obPrefix, tankNo, buyerType, remarks, releaseEntries, new List<BowserEntryDto>(), Guid.Empty);

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => orderSvc.CreateAsync(dto));

                      Assert.Equal(expectedError, ex.Message);
                  });
            }

            [Fact]
            public async Task WhenCustomerIsNotMatchingInEntries_ThrowsException()
            {
                var jkcs_customerId = Guid.Empty;
                var go_productId = Guid.Empty;
                var approvalId = Guid.Empty;

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      jkcs_customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JKCS");
                      go_productId = await EntityHelper.GetProductIdAsync(dbContext, "GO");
                      approvalId = await EntityHelper.GetGetEntryTxnIdAsync(dbContext, "1001", ApprovalType.Rebond, "50000");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = new List<OrderStockReleaseEntryDto>
                      {
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), ApprovalId=approvalId, EntryNo="1001", ObRef="xyz", Quantity = 100.1250m, DeliveredQuantity=0 }
                      };

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jkcs_customerId, go_productId, 1001,
                          new DateTime(2022, 8, 27), 100.1250m, null, "Dialog",
                          OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge,
                          null, releaseEntries, new List<BowserEntryDto>(), Guid.Empty);

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => orderSvc.CreateAsync(dto));

                      Assert.Equal("Customer miss-matching entries: 1001", ex.Message);
                  });
            }

            [Fact]
            public async Task WhenProductOsNotMatchingInEntries_ThrowsException()
            {
                var jvc_customerId = Guid.Empty;
                var lsfo_productId = Guid.Empty;
                var approvalId = Guid.Empty;

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      jvc_customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      lsfo_productId = await EntityHelper.GetProductIdAsync(dbContext, "380_LSFO");
                      approvalId = await EntityHelper.GetGetEntryTxnIdAsync(dbContext, "1001", ApprovalType.Rebond, "50000");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = new List<OrderStockReleaseEntryDto>
                      {
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="1001", ApprovalId=approvalId, ObRef="xyz", Quantity = 100.1250m, DeliveredQuantity=0 }
                      };

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jvc_customerId, lsfo_productId, 1001,
                          new DateTime(2022, 8, 27), 100.1250m, null, "Dialog",
                          OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge,
                          null, releaseEntries, new List<BowserEntryDto>(), Guid.Empty);

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => orderSvc.CreateAsync(dto));

                      Assert.Equal("Product miss-matching entries: 1001", ex.Message);
                  });
            }

            [Fact]
            public async Task WhenThereAreInvalidEntryNos_ThrowsException()
            {
                var jvc_customerId = Guid.Empty;
                var go_productId = Guid.Empty;
                var approvalId = Guid.Empty;

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      jvc_customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      go_productId = await EntityHelper.GetProductIdAsync(dbContext, "GO");
                      approvalId = await EntityHelper.GetGetEntryTxnIdAsync(dbContext, "1001", ApprovalType.Rebond, "50000");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = new List<OrderStockReleaseEntryDto>
                      {
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="2001", ApprovalId=approvalId, ObRef="xyz", Quantity = 100.1250m, DeliveredQuantity=0 }
                      };

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jvc_customerId, go_productId, 1001,
                          new DateTime(2022, 8, 27), 100.1250m, null, "Dialog",
                          OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge,
                          null, releaseEntries, new List<BowserEntryDto>(), Guid.Empty);

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => orderSvc.CreateAsync(dto));

                      Assert.Equal("Invalid entries: 2001", ex.Message);
                  });
            }

            [Fact]
            public async Task WhenThereAreMultipleInvalidEntryNos_ThrowsException()
            {
                var jvc_customerId = Guid.Empty;
                var go_productId = Guid.Empty;
                var approvalId = Guid.Empty;


                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      jvc_customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      go_productId = await EntityHelper.GetProductIdAsync(dbContext, "GO");
                      approvalId = await EntityHelper.GetGetEntryTxnIdAsync(dbContext, "1001", ApprovalType.Rebond, "50000");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = new List<OrderStockReleaseEntryDto>
                      {
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="1001", ApprovalId=approvalId, ObRef="xyz", Quantity = 120, DeliveredQuantity=0 },
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="3001", ApprovalId=approvalId, ObRef="xyz", Quantity = 120, DeliveredQuantity=0 },
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="2001", ApprovalId=approvalId, ObRef="xyz", Quantity = 120, DeliveredQuantity=0 }
                      };

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jvc_customerId, go_productId, 1001,
                          new DateTime(2022, 8, 27), 360, null, "Dialog",
                          OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge,
                          null, releaseEntries, new List<BowserEntryDto>(), Guid.Empty);

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => orderSvc.CreateAsync(dto));

                      Assert.Equal("Invalid entries: 2001|3001", ex.Message);
                  });
            }

            [Fact]
            public async Task WhenRequestedQuantitiesAreGreaterThanRemainingQuantity_ThrowsException()
            {
                var jvc_customerId = Guid.Empty;
                var go_productId = Guid.Empty;
                var approvalId = Guid.Empty;

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      jvc_customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      go_productId = await EntityHelper.GetProductIdAsync(dbContext, "GO");
                      approvalId = await EntityHelper.GetGetEntryTxnIdAsync(dbContext, "1001", ApprovalType.Rebond, "50000");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = new List<OrderStockReleaseEntryDto>
                      {
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="1001", ApprovalId=approvalId, ObRef="xyz", Quantity = 1500, DeliveredQuantity=0 },
                      };

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jvc_customerId, go_productId, 1001,
                          new DateTime(2022, 8, 27), 1500, null, "Dialog",
                          OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge,
                          null, releaseEntries, new List<BowserEntryDto>(), Guid.Empty);

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => orderSvc.CreateAsync(dto));

                      Assert.Equal("Remaining quantity (1000.25) of entry: 1001 is not sufficient to deliver: 1500, Remaining quantity (1000.25) of Rebond-50000 is not sufficient to deliver 1500", ex.Message);
                  });
            }

            [Fact]
            public async Task WhenThereAreCompletedEntriesSelected_ThrowsException()
            {
                var jvc_customerId = Guid.Empty;
                var lsfo_productId = Guid.Empty;
                var approvalId = Guid.Empty;

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      jvc_customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      lsfo_productId = await EntityHelper.GetProductIdAsync(dbContext, "380_LSFO");
                      approvalId = await EntityHelper.GetGetEntryTxnIdAsync(dbContext, "1104", ApprovalType.Rebond, "15344");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = new List<OrderStockReleaseEntryDto>
                      {
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="1104", ApprovalId=approvalId, ObRef="abc", Quantity = 5, DeliveredQuantity=0 },
                      };

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jvc_customerId, lsfo_productId, 2001,
                          new DateTime(2022, 8, 27), 5, null, "Mobitel",
                          OrderStatus.Undelivered, "OB-2", "100", BuyerType.Barge,
                          null, releaseEntries, new List<BowserEntryDto>(), Guid.Empty);

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => orderSvc.CreateAsync(dto));

                      Assert.Equal("Completed entries: 1104 cannot be used", ex.Message);
                  });
            }

            [Fact]
            public async Task WhenThereAreMultipleIssuesWithEntries_ThrowsException()
            {
                var jkcs_customerId = Guid.Empty;
                var lsfo_productId = Guid.Empty;
                var approvalId = Guid.Empty;

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      jkcs_customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JKCS");
                      lsfo_productId = await EntityHelper.GetProductIdAsync(dbContext, "380_LSFO");
                      approvalId = await EntityHelper.GetGetEntryTxnIdAsync(dbContext, "1001", ApprovalType.Rebond, "50000");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = new List<OrderStockReleaseEntryDto>
                      {
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="1001", ApprovalId=approvalId, ObRef="xyz", Quantity = 1500, DeliveredQuantity=0 },
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="3001", ApprovalId=approvalId, ObRef="xyz", Quantity = 120, DeliveredQuantity=0 },
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="2001", ApprovalId=approvalId, ObRef="xyz", Quantity = 120, DeliveredQuantity=0 }
                      };

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jkcs_customerId, lsfo_productId, 1001,
                          new DateTime(2022, 8, 27), 1740, null, "Dialog",
                          OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge,
                          null, releaseEntries, new List<BowserEntryDto>(), Guid.Empty);

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => orderSvc.CreateAsync(dto));

                      Assert.Equal("Invalid entries: 2001|3001, Product miss-matching entries: 1001, Customer miss-matching entries: 1001, Remaining quantity (1000.25) of entry: 1001 is not sufficient to deliver: 1500, Remaining quantity (1000.25) of Rebond-50000 is not sufficient to deliver 1500", ex.Message);
                  });
            }

            [Fact]
            public async Task WhenPassingCorrectData_CreatedSuccessfully()
            {
                var id = Guid.Empty;
                var jvc_customerId = Guid.Empty;
                var go_productId = Guid.Empty;
                var approvalId = Guid.Empty;

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      jvc_customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      go_productId = await EntityHelper.GetProductIdAsync(dbContext, "GO");
                      approvalId = await EntityHelper.GetGetEntryTxnIdAsync(dbContext, "1001", ApprovalType.Rebond, "50000");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = new List<OrderStockReleaseEntryDto>
                      {
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="1001", ApprovalId=approvalId, ObRef="xyz", Quantity = 120, DeliveredQuantity=0 }
                      };

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jvc_customerId, go_productId, 1,
                          new DateTime(2022, 8, 27), 120, null, "Dialog",
                          OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge,
                          "First order", releaseEntries, new List<BowserEntryDto>(), Guid.Empty);

                      var result = await orderSvc.CreateAsync(dto);

                      id = result.Id;
                  },
                   async (IDataContext dbContext) =>
                   {
                       var order = await dbContext.Orders.FindAsync(id);
                       var orderTxns = await dbContext.EntryTransactions.Where(t => t.OrderId == id).ToListAsync();
                       var entry = await dbContext.Entries.Where(e => e.EntryNo == "1001").FirstAsync();

                       Assert.NotNull(order);
                       Assert.Single(orderTxns);
                       Assert.NotNull(entry);

                       //Order
                       Assert.Equal(1, order.OrderNo);
                       Assert.Equal(120, order.Quantity);
                       Assert.Equal("Dialog", order.Buyer);
                       Assert.Equal(BuyerType.Barge, order.BuyerType);
                       Assert.Equal(OrderStatus.Undelivered, order.Status);
                       Assert.Equal("100", order.TankNo);
                       Assert.Equal(jvc_customerId, order.CustomerId);
                       Assert.Equal(go_productId, order.ProductId);
                       Assert.Equal("First order", order.Remarks);
                       Assert.Equal(new DateTime(2022, 8, 27), order.OrderDate);
                       Assert.Equal("OB-1", order.ObRefPrefix);

                       //Entry Transactions
                       var txn1 = orderTxns.First();

                       Assert.Equal("xyz", txn1.ObRef);
                       Assert.Equal(entry.Id, txn1.EntryId);
                       Assert.Equal(-120, txn1.Quantity);
                       Assert.Null(txn1.DeliveredQuantity);
                       Assert.Equal(EntryTransactionType.Out, txn1.Type);
                       Assert.Equal(approvalId, txn1.ApprovalTransactionId);

                       //Entry
                       Assert.Equal(1000.250m, entry.InitialQualtity);
                       Assert.Equal(880.250m, entry.RemainingQuantity);
                       Assert.Equal(EntryStatus.Active, entry.Status);
                   });
            }

            [Fact]
            public async Task WhenEntryRemQtyIsZeroEntryMardAsCompleted_CreatedSuccessfully()
            {
                var id = Guid.Empty;
                var jvc_customerId = Guid.Empty;
                var go_productId = Guid.Empty;
                var approvalId = Guid.Empty;

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      jvc_customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      go_productId = await EntityHelper.GetProductIdAsync(dbContext, "GO");
                      approvalId = await EntityHelper.GetGetEntryTxnIdAsync(dbContext, "1001", ApprovalType.Rebond, "50000");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = new List<OrderStockReleaseEntryDto>
                      {
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="1001", ApprovalId=approvalId, ObRef="xyz", Quantity = 1000.250m, DeliveredQuantity=0 }
                      };

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jvc_customerId, go_productId, 1,
                          new DateTime(2022, 8, 27), 1000.250m, null, "Dialog",
                          OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge,
                          "First order", releaseEntries, new List<BowserEntryDto>(), Guid.Empty);

                      var result = await orderSvc.CreateAsync(dto);

                      id = result.Id;
                  },
                   async (IDataContext dbContext) =>
                   {
                       var order = await dbContext.Orders.FindAsync(id);
                       var orderTxns = await dbContext.EntryTransactions.Where(t => t.OrderId == id).ToListAsync();
                       var entry = await dbContext.Entries.Where(e => e.EntryNo == "1001").FirstAsync();

                       Assert.NotNull(order);
                       Assert.Single(orderTxns);
                       Assert.NotNull(entry);

                       //Order
                       Assert.Equal(1, order.OrderNo);
                       Assert.Equal(1000.250m, order.Quantity);
                       Assert.Equal("Dialog", order.Buyer);
                       Assert.Equal(BuyerType.Barge, order.BuyerType);
                       Assert.Equal(OrderStatus.Undelivered, order.Status);
                       Assert.Equal("100", order.TankNo);
                       Assert.Equal(jvc_customerId, order.CustomerId);
                       Assert.Equal(go_productId, order.ProductId);
                       Assert.Equal("First order", order.Remarks);
                       Assert.Equal(new DateTime(2022, 8, 27), order.OrderDate);
                       Assert.Equal("OB-1", order.ObRefPrefix);

                       //Entry Transactions
                       var txn1 = orderTxns.First();

                       Assert.Equal("xyz", txn1.ObRef);
                       Assert.Equal(entry.Id, txn1.EntryId);
                       Assert.Equal(-1000.250m, txn1.Quantity);
                       Assert.Null(txn1.DeliveredQuantity);
                       Assert.Equal(EntryTransactionType.Out, txn1.Type);
                       Assert.Equal(approvalId, txn1.ApprovalTransactionId);

                       //Entry
                       Assert.Equal(1000.250m, entry.InitialQualtity);
                       Assert.Equal(0, entry.RemainingQuantity);
                       Assert.Equal(EntryStatus.Active, entry.Status);
                   });
            }
        }

        public class Update
        {
            [Fact]
            public async Task WhenPassingValidData_UpdatesSuccessfully()
            {
                var jvc_customerId = Guid.Empty;
                var lfso_productId = Guid.Empty;
                Order order = null;

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      jvc_customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      lfso_productId = await EntityHelper.GetProductIdAsync(dbContext, "380_LSFO");

                      order = await dbContext.Orders
                        .Where(o => o.OrderNo == 1502)
                        .Include(o => o.Transactions).ThenInclude(t => t.Entry)
                        .Include(o => o.BowserEntries)
                        .SingleOrDefaultAsync();
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = order.Transactions.Select(t => new OrderStockReleaseEntryDto
                      {
                          Id = t.Id,
                          EntryNo = t.Entry.EntryNo,
                          ApprovalId = t.ApprovalTransactionId.Value,
                          ObRef = "ref-11-a",
                          Quantity = Math.Abs(t.Quantity),
                          DeliveredQuantity = t.DeliveredQuantity != null ? Math.Abs(t.DeliveredQuantity.Value) : null,
                      }).ToList();

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jvc_customerId, lfso_productId, 1602,
                          new DateTime(2022, 8, 27), 100, null, "Mobitel",
                          OrderStatus.Undelivered, "OB/2023", "100", BuyerType.Barge,
                          "First order new", releaseEntries, new List<BowserEntryDto>(), order.ConcurrencyKey);

                      var result = await orderSvc.UpdateAsync(order.Id, dto);
                  },
                   async (IDataContext dbContext) =>
                   {
                       var updatedOrder = await dbContext.Orders.FindAsync(order.Id);
                       var orderTxns = await dbContext.EntryTransactions.Where(t => t.OrderId == order.Id).ToListAsync();
                       var entry = await dbContext.Entries.Where(e => e.EntryNo == "1002").FirstAsync();

                       Assert.NotNull(updatedOrder);
                       Assert.Single(orderTxns);
                       Assert.NotNull(entry);

                       //Order
                       Assert.Equal(1602, updatedOrder.OrderNo);
                       Assert.Equal(100, updatedOrder.Quantity);
                       Assert.Equal("Mobitel", updatedOrder.Buyer);
                       Assert.Equal(BuyerType.Barge, updatedOrder.BuyerType);
                       Assert.Equal(OrderStatus.Undelivered, updatedOrder.Status);
                       Assert.Equal("100", updatedOrder.TankNo);
                       Assert.Equal(jvc_customerId, updatedOrder.CustomerId);
                       Assert.Equal(lfso_productId, updatedOrder.ProductId);
                       Assert.Equal("First order new", updatedOrder.Remarks);
                       Assert.Equal(new DateTime(2022, 8, 27), updatedOrder.OrderDate);
                       Assert.Equal("OB/2023", updatedOrder.ObRefPrefix);

                       //Entry Transactions
                       var txn1 = orderTxns.First();

                       Assert.Equal("ref-11-a", txn1.ObRef);
                       Assert.Equal(entry.Id, txn1.EntryId);
                       Assert.Equal(-100, txn1.Quantity);
                       Assert.Null(txn1.DeliveredQuantity);
                       Assert.Equal(EntryTransactionType.Out, txn1.Type);
                       Assert.Equal(updatedOrder.OrderDate, txn1.TransactionDate);

                       //Entry
                       Assert.Equal(500, entry.InitialQualtity);
                       Assert.Equal(220, entry.RemainingQuantity);
                       Assert.Equal(EntryStatus.Active, entry.Status);
                   });
            }

            [Fact]
            public async Task WhenModifyingOrderQuantities_UpdatesSuccessfully()
            {
                var jvc_customerId = Guid.Empty;
                var lfso_productId = Guid.Empty;
                Order order = null;

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      jvc_customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      lfso_productId = await EntityHelper.GetProductIdAsync(dbContext, "380_LSFO");

                      order = await dbContext.Orders
                        .Where(o => o.OrderNo == 1502)
                        .Include(o => o.Transactions).ThenInclude(t => t.Entry)
                        .Include(o => o.BowserEntries)
                        .SingleOrDefaultAsync();
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = order.Transactions.Select(t => new OrderStockReleaseEntryDto
                      {
                          Id = t.Id,
                          EntryNo = t.Entry.EntryNo,
                          ApprovalId = t.ApprovalTransactionId.Value,
                          ObRef = "ref-11-a",
                          Quantity = 150,
                          DeliveredQuantity = t.DeliveredQuantity != null ? Math.Abs(t.DeliveredQuantity.Value) : null,
                      }).ToList();

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jvc_customerId, lfso_productId, 1502,
                          new DateTime(2022, 8, 28), 150, null, "Mobitel",
                          OrderStatus.Undelivered, "OB/2023", "100", BuyerType.Barge,
                          "First order", releaseEntries, new List<BowserEntryDto>(), order.ConcurrencyKey);

                      var result = await orderSvc.UpdateAsync(order.Id, dto);
                  },
                   async (IDataContext dbContext) =>
                   {
                       var updatedOrder = await dbContext.Orders.FindAsync(order.Id);
                       var orderTxns = await dbContext.EntryTransactions.Where(t => t.OrderId == order.Id).ToListAsync();
                       var entry = await dbContext.Entries.Where(e => e.EntryNo == "1002").FirstAsync();

                       Assert.NotNull(updatedOrder);
                       Assert.Single(orderTxns);
                       Assert.NotNull(entry);

                       //Order
                       Assert.Equal(1502, updatedOrder.OrderNo);
                       Assert.Equal(150, updatedOrder.Quantity);
                       Assert.Equal("Mobitel", updatedOrder.Buyer);
                       Assert.Equal(BuyerType.Barge, updatedOrder.BuyerType);
                       Assert.Equal(OrderStatus.Undelivered, updatedOrder.Status);
                       Assert.Equal("100", updatedOrder.TankNo);
                       Assert.Equal(jvc_customerId, updatedOrder.CustomerId);
                       Assert.Equal(lfso_productId, updatedOrder.ProductId);
                       Assert.Equal("First order", updatedOrder.Remarks);
                       Assert.Equal(new DateTime(2022, 8, 28), updatedOrder.OrderDate);
                       Assert.Equal("OB/2023", updatedOrder.ObRefPrefix);

                       //Entry Transactions
                       var txn1 = orderTxns.First();

                       Assert.Equal("ref-11-a", txn1.ObRef);
                       Assert.Equal(entry.Id, txn1.EntryId);
                       Assert.Equal(-150, txn1.Quantity);
                       Assert.Null(txn1.DeliveredQuantity);
                       Assert.Equal(EntryTransactionType.Out, txn1.Type);
                       Assert.Equal(updatedOrder.OrderDate, txn1.TransactionDate);

                       //Entry
                       Assert.Equal(500, entry.InitialQualtity);
                       Assert.Equal(170, entry.RemainingQuantity);
                       Assert.Equal(EntryStatus.Active, entry.Status);
                   });
            }

            [Fact]
            public async Task WhenModifyingEntry_UpdatesSuccessfully()
            {
                var jvc_customerId = Guid.Empty;
                var lfso_productId = Guid.Empty;
                var approvalId = Guid.Empty;
                Order order = null;

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      jvc_customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      lfso_productId = await EntityHelper.GetProductIdAsync(dbContext, "380_LSFO");
                      approvalId = await EntityHelper.GetGetEntryTxnIdAsync(dbContext, "1103", ApprovalType.Rebond, "15244");

                      order = await dbContext.Orders
                        .Where(o => o.OrderNo == 1502)
                        .Include(o => o.Transactions).ThenInclude(t => t.Entry)
                        .Include(o => o.BowserEntries)
                        .SingleOrDefaultAsync();
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = order.Transactions.Select(t => new OrderStockReleaseEntryDto
                      {
                          Id = t.Id,
                          EntryNo = "1103",
                          ApprovalId = approvalId,
                          ObRef = "ref-11-a",
                          Quantity = 190,
                          DeliveredQuantity = t.DeliveredQuantity != null ? Math.Abs(t.DeliveredQuantity.Value) : null,
                      }).ToList();

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jvc_customerId, lfso_productId, 1502,
                          new DateTime(2022, 8, 28), 190, null, "Mobitel",
                          OrderStatus.Undelivered, "OB/2023", "100", BuyerType.Barge,
                          "First order", releaseEntries, new List<BowserEntryDto>(), order.ConcurrencyKey);

                      var result = await orderSvc.UpdateAsync(order.Id, dto);
                  },
                   async (IDataContext dbContext) =>
                   {
                       var updatedOrder = await dbContext.Orders.FindAsync(order.Id);
                       var orderTxns = await dbContext.EntryTransactions.Where(t => t.OrderId == order.Id).ToListAsync();
                       var oldEntry = await dbContext.Entries.Where(e => e.EntryNo == "1002").FirstAsync();
                       var newEntry = await dbContext.Entries.Where(e => e.EntryNo == "1103").FirstAsync();

                       Assert.NotNull(updatedOrder);
                       Assert.Single(orderTxns);
                       Assert.NotNull(oldEntry);
                       Assert.NotNull(newEntry);

                       //Order
                       Assert.Equal(1502, updatedOrder.OrderNo);
                       Assert.Equal(190, updatedOrder.Quantity);
                       Assert.Equal("Mobitel", updatedOrder.Buyer);
                       Assert.Equal(BuyerType.Barge, updatedOrder.BuyerType);
                       Assert.Equal(OrderStatus.Undelivered, updatedOrder.Status);
                       Assert.Equal("100", updatedOrder.TankNo);
                       Assert.Equal(jvc_customerId, updatedOrder.CustomerId);
                       Assert.Equal(lfso_productId, updatedOrder.ProductId);
                       Assert.Equal("First order", updatedOrder.Remarks);
                       Assert.Equal(new DateTime(2022, 8, 28), updatedOrder.OrderDate);
                       Assert.Equal("OB/2023", updatedOrder.ObRefPrefix);

                       //Entry Transactions
                       var txn1 = orderTxns.First();

                       Assert.Equal("ref-11-a", txn1.ObRef);
                       Assert.Equal(newEntry.Id, txn1.EntryId);
                       Assert.Equal(-190, txn1.Quantity);
                       Assert.Null(txn1.DeliveredQuantity);
                       Assert.Equal(EntryTransactionType.Out, txn1.Type);
                       Assert.Equal(updatedOrder.OrderDate, txn1.TransactionDate);

                       //Old Entry
                       Assert.Equal(500, oldEntry.InitialQualtity);
                       Assert.Equal(320, oldEntry.RemainingQuantity);
                       Assert.Equal(EntryStatus.Active, oldEntry.Status);

                       //New Entry
                       Assert.Equal(200, newEntry.InitialQualtity);
                       Assert.Equal(10, newEntry.RemainingQuantity);
                       Assert.Equal(EntryStatus.Active, newEntry.Status);
                   });
            }

            [Fact]
            public async Task WhenOrderMarkingAsDelivered_EntryCompleteSuccessfully()
            {
                var jvc_customerId = Guid.Empty;
                var lfso_productId = Guid.Empty;
                Order order = null;
                var approvalId = Guid.Empty;


                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      jvc_customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      lfso_productId = await EntityHelper.GetProductIdAsync(dbContext, "380_LSFO");
                      approvalId = await EntityHelper.GetGetEntryTxnIdAsync(dbContext, "1103", ApprovalType.Rebond, "15244");

                      order = await dbContext.Orders
                        .Where(o => o.OrderNo == 1502)
                        .Include(o => o.Transactions).ThenInclude(t => t.Entry)
                        .Include(o => o.BowserEntries)
                        .SingleOrDefaultAsync();
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = new List<OrderStockReleaseEntryDto>
                      {
                          new OrderStockReleaseEntryDto
                          {
                              Id = Guid.Empty,
                              EntryNo = "1103",
                              ApprovalId = approvalId,
                              ObRef = "ref-11-a",
                              Quantity = 200,
                              DeliveredQuantity = 200
                          }
                      };

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jvc_customerId, lfso_productId, 1502,
                          new DateTime(2022, 8, 28), 200, 200, "Mobitel",
                          OrderStatus.Delivered, "OB/2023", "100", BuyerType.Barge,
                          "First order", releaseEntries, new List<BowserEntryDto>(), order.ConcurrencyKey);

                      var result = await orderSvc.UpdateAsync(order.Id, dto);
                  },
                   async (IDataContext dbContext) =>
                   {
                       var updatedOrder = await dbContext.Orders.FindAsync(order.Id);
                       var orderTxns = await dbContext.EntryTransactions.Where(t => t.OrderId == order.Id).ToListAsync();
                       var oldEntry = await dbContext.Entries.Where(e => e.EntryNo == "1002").FirstAsync();
                       var newEntry = await dbContext.Entries.Where(e => e.EntryNo == "1103").FirstAsync();

                       Assert.NotNull(updatedOrder);
                       Assert.Single(orderTxns);
                       Assert.NotNull(oldEntry);
                       Assert.NotNull(newEntry);

                       //Order
                       Assert.Equal(1502, updatedOrder.OrderNo);
                       Assert.Equal(200, updatedOrder.Quantity);
                       Assert.Equal(200, updatedOrder.DeliveredQuantity);
                       Assert.Equal("Mobitel", updatedOrder.Buyer);
                       Assert.Equal(BuyerType.Barge, updatedOrder.BuyerType);
                       Assert.Equal(OrderStatus.Delivered, updatedOrder.Status);
                       Assert.Equal("100", updatedOrder.TankNo);
                       Assert.Equal(jvc_customerId, updatedOrder.CustomerId);
                       Assert.Equal(lfso_productId, updatedOrder.ProductId);
                       Assert.Equal("First order", updatedOrder.Remarks);
                       Assert.Equal(new DateTime(2022, 8, 28), updatedOrder.OrderDate);
                       Assert.Equal("OB/2023", updatedOrder.ObRefPrefix);

                       //Entry Transactions
                       var txn1 = orderTxns.First();

                       Assert.Equal("ref-11-a", txn1.ObRef);
                       Assert.Equal(newEntry.Id, txn1.EntryId);
                       Assert.Equal(-200, txn1.Quantity);
                       Assert.Equal(-200, txn1.DeliveredQuantity);
                       Assert.Equal(EntryTransactionType.Out, txn1.Type);
                       Assert.Equal(updatedOrder.OrderDate, txn1.TransactionDate);

                       //Old Entry
                       Assert.Equal(500, oldEntry.InitialQualtity);
                       Assert.Equal(320, oldEntry.RemainingQuantity);
                       Assert.Equal(EntryStatus.Active, oldEntry.Status);

                       //New Entry
                       Assert.Equal(200, newEntry.InitialQualtity);
                       Assert.Equal(0, newEntry.RemainingQuantity);
                       Assert.Equal(EntryStatus.Completed, newEntry.Status);
                   });
            }

            [Fact]
            public async Task WhenOrderNotMarkingAsDelivered_EntryStillWillActive()
            {
                var jvc_customerId = Guid.Empty;
                var lfso_productId = Guid.Empty;
                var approvalId = Guid.Empty;

                Order order = null;

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      jvc_customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      lfso_productId = await EntityHelper.GetProductIdAsync(dbContext, "380_LSFO");
                      approvalId = await EntityHelper.GetGetEntryTxnIdAsync(dbContext, "1103", ApprovalType.Rebond, "15244");

                      order = await dbContext.Orders
                        .Where(o => o.OrderNo == 1502)
                        .Include(o => o.Transactions).ThenInclude(t => t.Entry)
                        .Include(o => o.BowserEntries)
                        .SingleOrDefaultAsync();
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = new List<OrderStockReleaseEntryDto>
                      {
                          new OrderStockReleaseEntryDto
                          {
                              Id = Guid.Empty,
                              EntryNo = "1103",
                              ApprovalId=approvalId,
                              ObRef = "ref-11-a",
                              Quantity = 200,
                              DeliveredQuantity = 200
                          }
                      };

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jvc_customerId, lfso_productId, 1502,
                          new DateTime(2022, 8, 28), 200, 200, "Mobitel",
                          OrderStatus.Undelivered, "OB/2023", "100", BuyerType.Barge,
                          "First order", releaseEntries, new List<BowserEntryDto>(), order.ConcurrencyKey);

                      var result = await orderSvc.UpdateAsync(order.Id, dto);
                  },
                   async (IDataContext dbContext) =>
                   {
                       var updatedOrder = await dbContext.Orders.FindAsync(order.Id);
                       var orderTxns = await dbContext.EntryTransactions.Where(t => t.OrderId == order.Id).ToListAsync();
                       var oldEntry = await dbContext.Entries.Where(e => e.EntryNo == "1002").FirstAsync();
                       var newEntry = await dbContext.Entries.Where(e => e.EntryNo == "1103").FirstAsync();

                       Assert.NotNull(updatedOrder);
                       Assert.Single(orderTxns);
                       Assert.NotNull(oldEntry);
                       Assert.NotNull(newEntry);

                       //Order
                       Assert.Equal(1502, updatedOrder.OrderNo);
                       Assert.Equal(200, updatedOrder.Quantity);
                       Assert.Null(updatedOrder.DeliveredQuantity);
                       Assert.Equal("Mobitel", updatedOrder.Buyer);
                       Assert.Equal(BuyerType.Barge, updatedOrder.BuyerType);
                       Assert.Equal(OrderStatus.Undelivered, updatedOrder.Status);
                       Assert.Equal("100", updatedOrder.TankNo);
                       Assert.Equal(jvc_customerId, updatedOrder.CustomerId);
                       Assert.Equal(lfso_productId, updatedOrder.ProductId);
                       Assert.Equal("First order", updatedOrder.Remarks);
                       Assert.Equal(new DateTime(2022, 8, 28), updatedOrder.OrderDate);
                       Assert.Equal("OB/2023", updatedOrder.ObRefPrefix);

                       //Entry Transactions
                       var txn1 = orderTxns.First();

                       Assert.Equal("ref-11-a", txn1.ObRef);
                       Assert.Equal(newEntry.Id, txn1.EntryId);
                       Assert.Equal(-200, txn1.Quantity);
                       Assert.Null(txn1.DeliveredQuantity);
                       Assert.Equal(EntryTransactionType.Out, txn1.Type);
                       Assert.Equal(updatedOrder.OrderDate, txn1.TransactionDate);

                       //Old Entry
                       Assert.Equal(500, oldEntry.InitialQualtity);
                       Assert.Equal(320, oldEntry.RemainingQuantity);
                       Assert.Equal(EntryStatus.Active, oldEntry.Status);

                       //New Entry
                       Assert.Equal(200, newEntry.InitialQualtity);
                       Assert.Equal(0, newEntry.RemainingQuantity);
                       Assert.Equal(EntryStatus.Active, newEntry.Status);
                   });
            }

            [Fact]
            public async Task WhenDeliveredOrderMarkingAsNotDelivered_EntryBecomeActive()
            {
                var jvc_customerId = Guid.Empty;
                var lfso_productId = Guid.Empty;
                Order order = null;

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      jvc_customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      lfso_productId = await EntityHelper.GetProductIdAsync(dbContext, "380_LSFO");

                      order = await dbContext.Orders
                        .Where(o => o.OrderNo == 1501)
                        .Include(o => o.Transactions).ThenInclude(t => t.Entry)
                        .Include(o => o.BowserEntries)
                        .SingleOrDefaultAsync();
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = order.Transactions.Select(t => new OrderStockReleaseEntryDto
                      {
                          Id = t.Id,
                          EntryNo = t.Entry.EntryNo,
                          ApprovalId = t.ApprovalTransactionId.Value,
                          ObRef = "ref-11-a",
                          Quantity = Math.Abs(t.Quantity),
                          DeliveredQuantity = t.DeliveredQuantity != null ? Math.Abs(t.DeliveredQuantity.Value) : null,
                      }).ToList();

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jvc_customerId, lfso_productId, 1501,
                          new DateTime(2022, 8, 28), 199.5m, 190, "Mobitel",
                          OrderStatus.Undelivered, "OB/2023", "100", BuyerType.Barge,
                          "First order", releaseEntries, new List<BowserEntryDto>(), order.ConcurrencyKey);

                      var result = await orderSvc.UpdateAsync(order.Id, dto);
                  },
                   async (IDataContext dbContext) =>
                   {
                       var updatedOrder = await dbContext.Orders.FindAsync(order.Id);
                       var entry = await dbContext.Entries.Where(e => e.EntryNo == "1104").FirstAsync();

                       Assert.NotNull(updatedOrder);
                       Assert.NotNull(entry);

                       //Old Entry
                       Assert.Equal(10, entry.InitialQualtity);
                       Assert.Equal(0, entry.RemainingQuantity);
                       Assert.Equal(EntryStatus.Active, entry.Status);
                   });
            }

            [Fact]
            public async Task WhenRemainingApprovedQtyIsNotSufficient_ThrowsException()
            {
                var jvc_customerId = Guid.Empty;
                var lfso_productId = Guid.Empty;
                Order order = null;

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      jvc_customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      lfso_productId = await EntityHelper.GetProductIdAsync(dbContext, "380_LSFO");

                      order = await dbContext.Orders
                        .Where(o => o.OrderNo == 1502)
                        .Include(o => o.Transactions).ThenInclude(t => t.Entry)
                        .Include(o => o.BowserEntries)
                        .SingleOrDefaultAsync();
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = order.Transactions.Select(t => new OrderStockReleaseEntryDto
                      {
                          Id = t.Id,
                          EntryNo = t.Entry.EntryNo,
                          ApprovalId = t.ApprovalTransactionId.Value,
                          ObRef = t.ObRef,
                          Quantity = 171,
                          DeliveredQuantity = t.DeliveredQuantity != null ? Math.Abs(t.DeliveredQuantity.Value) : null,
                      }).ToList();

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jvc_customerId, lfso_productId, 1602,
                          new DateTime(2022, 8, 27), 171, null, "Mobitel",
                          OrderStatus.Undelivered, "OB/2023", "100", BuyerType.Barge,
                          "First order new", releaseEntries, new List<BowserEntryDto>(), order.ConcurrencyKey);

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => orderSvc.UpdateAsync(order.Id, dto));

                      Assert.Equal("Remaining quantity (170) of XBond-60000 is not sufficient to deliver 171", ex.Message);
                  });
            }

            [Fact]
            public async Task WhenRemainingQtyIsNotSufficient_ThrowsException()
            {
                var jvc_customerId = Guid.Empty;
                var lfso_productId = Guid.Empty;
                Order order = null;

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      jvc_customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      lfso_productId = await EntityHelper.GetProductIdAsync(dbContext, "380_LSFO");

                      order = await dbContext.Orders
                        .Where(o => o.OrderNo == 1502)
                        .Include(o => o.Transactions).ThenInclude(t => t.Entry)
                        .Include(o => o.BowserEntries)
                        .SingleOrDefaultAsync();
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = order.Transactions.Select(t => new OrderStockReleaseEntryDto
                      {
                          Id = t.Id,
                          EntryNo = t.Entry.EntryNo,
                          ApprovalId = t.ApprovalTransactionId.Value,
                          ObRef = t.ObRef,
                          Quantity = 321,
                          DeliveredQuantity = t.DeliveredQuantity != null ? Math.Abs(t.DeliveredQuantity.Value) : null,
                      }).ToList();

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jvc_customerId, lfso_productId, 1602,
                          new DateTime(2022, 8, 27), 321, null, "Mobitel",
                          OrderStatus.Undelivered, "OB/2023", "100", BuyerType.Barge,
                          "First order new", releaseEntries, new List<BowserEntryDto>(), order.ConcurrencyKey);

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => orderSvc.UpdateAsync(order.Id, dto));

                      Assert.Equal("Remaining quantity (320) of entry: 1002 is not sufficient to deliver: 321, Remaining quantity (170) of XBond-60000 is not sufficient to deliver 321", ex.Message);
                  });
            }
        }

        public class Delete
        {
            [Fact]
            public async Task WhenDeletingOrder_UpdateEntriesSuccessfully()
            {
                var orderId = Guid.Empty;
                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      orderId = await EntityHelper.GetOrderIdAsync(dbContext, 1502);
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      await orderSvc.DeleteAsync(orderId);
                  },
                  async (IDataContext dbContext) =>
                  {
                      var order = await dbContext.Orders.FindAsync(orderId);
                      var entry = await dbContext.Entries
                           .Where(e => e.EntryNo == "1002")
                           .Include(e => e.Transactions)
                           .FirstAsync();

                      Assert.Null(order);

                      //Entry Transactions
                      Assert.Equal(2, entry.Transactions.Count);
                      Assert.DoesNotContain(entry.Transactions, t => t.ObRef == "ref-11");

                      //Entry
                      Assert.Equal(500, entry.InitialQualtity);
                      Assert.Equal(320, entry.RemainingQuantity);
                      Assert.Equal(EntryStatus.Active, entry.Status);
                  });
            }

            [Fact]
            public async Task WhenDeletingDeliveredOrder_ThrowsException()
            {
                var orderId = Guid.Empty;
                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      orderId = await EntityHelper.GetOrderIdAsync(dbContext, 1501);
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => orderSvc.DeleteAsync(orderId));

                      Assert.Equal("Delivered orders cannot be deleted", ex.Message);
                  });
            }
        }

        public class Cancel
        {
            [Fact]
            public async Task WhenCancelingOrder_ReversalsCreatesSuccessfully()
            {
                var orderId = Guid.Empty;
                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      orderId = await EntityHelper.GetOrderIdAsync(dbContext, 1502);
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      await orderSvc.CancelOrderAsync(orderId);
                  },
                  async (IDataContext dbContext) =>
                  {
                      var order = await dbContext.Orders.FindAsync(orderId);
                      var entry = await dbContext.Entries
                           .Where(e => e.EntryNo == "1002")
                           .Include(e => e.Transactions)
                           .FirstAsync();

                      Assert.NotNull(order);
                      Assert.Equal(OrderStatus.Cancelled, order.Status);

                      Assert.Equal(320, entry.RemainingQuantity);

                      //Entry Transactions
                      Assert.Contains(entry.Transactions, t => t.OrderId == orderId && t.Type == EntryTransactionType.Reversal);

                      var reversal = entry.Transactions
                      .Where(t => t.OrderId == orderId && t.Type == EntryTransactionType.Reversal)
                      .FirstOrDefault();

                      //Entry
                      Assert.NotNull(reversal);
                      Assert.Equal(100, reversal.Quantity);
                  });
            }
        }

        public class Get
        {
            [Fact]
            public async Task WhenOrderExists_ReturnsCorrectData()
            {
                var id = Guid.Empty;
                var jvc_customerId = Guid.Empty;
                var lsfo_productId = Guid.Empty;
                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);
                      id = await EntityHelper.GetOrderIdAsync(dbContext, 1501);
                      jvc_customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      lsfo_productId = await EntityHelper.GetProductIdAsync(dbContext, "380_LSFO");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var order = await orderSvc.GetOrderAsync(id);

                      //Order
                      Assert.NotNull(order);

                      Assert.Equal(1501, order.OrderNo);
                      Assert.Equal(199.5m, order.Quantity);
                      Assert.Equal("Exex", order.Buyer);
                      Assert.Equal(BuyerType.Bowser, order.BuyerType);
                      Assert.Equal(OrderStatus.Delivered, order.Status);
                      Assert.Equal("110", order.TankNo);
                      Assert.Equal(jvc_customerId, order.CustomerId);
                      Assert.Equal(lsfo_productId, order.ProductId);
                      Assert.Equal("Test 123", order.Remarks);
                      Assert.Equal(new DateTime(2022, 8, 27), order.OrderDate);
                      Assert.Equal("OB/2022", order.ObRefPrefix);

                      //Entry Transactions
                      Assert.Equal(2, order.ReleaseEntries.Count);

                      var txn1 = order.ReleaseEntries.First(e => e.ObRef == "ref-10");

                      Assert.NotEqual(Guid.Empty, txn1.Id);
                      Assert.Equal("ref-10", txn1.ObRef);
                      Assert.Equal("1002", txn1.EntryNo);
                      Assert.Equal(189.5m, txn1.Quantity);
                      Assert.Equal(180, txn1.DeliveredQuantity);

                      var txn2 = order.ReleaseEntries.First(e => e.ObRef == "ref-21");

                      Assert.NotEqual(Guid.Empty, txn2.Id);
                      Assert.Equal("ref-21", txn2.ObRef);
                      Assert.Equal("1104", txn2.EntryNo);
                      Assert.Equal(10, txn2.Quantity);
                      Assert.Equal(10, txn2.DeliveredQuantity);

                      //Bowser Entries
                      Assert.Single(order.BowserEntries);

                      var b1 = order.BowserEntries.First();

                      Assert.NotEqual(Guid.Empty, b1.Id);
                      Assert.Equal(13600, b1.Capacity);
                      Assert.Equal(2, b1.Count);
                  });
            }

            [Fact]
            public async Task WhenOrderNotExists_ReturnsNull()
            {
                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var order = await orderSvc.GetOrderAsync(Guid.NewGuid());

                      Assert.Null(order);
                  });
            }
        }

        public class Search
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
                      var orderSvc = CreateService(dbContext);

                      var orders = await orderSvc.SearchOrdersAsync(new OrderSearchDto() { Page = 1, PageSize = 100 });

                      Assert.NotNull(orders);
                      Assert.True(orders.Total > 0);
                      Assert.Equal(orders.Total, orders.Items.Count);

                      var order = orders.Items.First(o => o.OrderNo == 1501);

                      Assert.Equal(1501, order.OrderNo);
                      Assert.Equal(199.5m, order.Quantity);
                      Assert.Equal("Exex", order.Buyer);
                      Assert.Equal(BuyerType.Bowser, order.BuyerType);
                      Assert.Equal(OrderStatus.Delivered, order.Status);
                      Assert.Equal("JVC", order.Customer);
                      Assert.Equal("380_LSFO", order.Product);
                      Assert.Equal(new DateTime(2022, 8, 27), order.OrderDate);
                  });
            }
        }

        private static async Task SetupTestDataAsync(IDataContext dbContext)
        {
            await TestData.Orders.SetupOrderAndEntryTestDataAsync(dbContext);
        }

        private static OrderService CreateService(IDataContext dataContext)
        {
            var featureToggles = Options.Create(new FeatureToggles { AllowEditingActiveEntries = false });
            var entrySvc = new EntryService(dataContext, featureToggles);
            var orderSvc = new OrderService(dataContext, entrySvc);
            return orderSvc;
        }
    }
}
