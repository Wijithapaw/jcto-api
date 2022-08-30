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

#pragma warning disable xUnit1012 // Null should not be used for value type parameters
namespace JCTO.Tests
{
    public class OrderServiceTests
    {
        public class Create
        {
            [Theory]
            [InlineData("1001", "2022-8-26", 111, "Dialog", OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge, "", null, "Sum of release Quantities not equal to overall Quantity")]
            [InlineData("1001", null, 110, "Dialog", OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge, "", null, "Order Date not found")]
            [InlineData("", "2022-8-26", 110, "Dialog", OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge, "", null, "Order No. not found")]
            [InlineData("1001", "2022-8-26", 110, "", OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge, "", null, "Buyer not found")]
            [InlineData("1001", "2022-8-26", 110, "Dialog", OrderStatus.Undelivered, "", "", BuyerType.Barge, "", null, "OBRef not found, Tank No. not found")]
            [InlineData("1001", "2022-8-26", 0, "Dialog", OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge, "", null, "Quantity must be > 0, Sum of release Quantities not equal to overall Quantity")]
            [InlineData("1001", "2022-8-26", 0, "", OrderStatus.Undelivered, "", "", BuyerType.Barge, "", null, "Buyer not found, Quantity must be > 0, OBRef not found, Tank No. not found, Sum of release Quantities not equal to overall Quantity")]
            public async Task WhenPassingValidData_CreateSuccessfully(string orderNo, DateTime orderDate, double quantity,
            string buyer, OrderStatus status, string obPrefix, string tankNo, BuyerType buyerType,
            string xBondNo, string remarks, string expectedError)
            {
                var customerId = Guid.Empty;
                var productId = Guid.Empty;

                var releaseEntries = new List<OrderStockReleaseEntryDto>
                {
                    new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="10001", ObRef="xyz", Quantity = 110, DeliveredQuantity=0 }
                };

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      productId = await EntityHelper.GetProductIdAsync(dbContext, "GO");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, customerId, productId, orderNo, orderDate, quantity,
                          buyer, status, obPrefix, tankNo, buyerType, xBondNo, remarks, releaseEntries, new List<BowserEntryDto>());

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => orderSvc.CreateAsync(dto));

                      Assert.Equal(expectedError, ex.Message);
                  });
            }

            [Fact]
            public async Task WhenCustomerIsNotMatchingInEntries_ThrowsException()
            {
                var jkcs_customerId = Guid.Empty;
                var go_productId = Guid.Empty;

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      jkcs_customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JKCS");
                      go_productId = await EntityHelper.GetProductIdAsync(dbContext, "GO");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = new List<OrderStockReleaseEntryDto>
                      {
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="1001", ObRef="xyz", Quantity = 100.1250, DeliveredQuantity=0 }
                      };

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jkcs_customerId, go_productId, "1001",
                          new DateTime(2022, 8, 27), 100.1250, "Dialog",
                          OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge,
                          "", null, releaseEntries, new List<BowserEntryDto>());

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => orderSvc.CreateAsync(dto));

                      Assert.Equal("Customer miss-matching entries: 1001", ex.Message);
                  });
            }

            [Fact]
            public async Task WhenProductOsNotMatchingInEntries_ThrowsException()
            {
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
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = new List<OrderStockReleaseEntryDto>
                      {
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="1001", ObRef="xyz", Quantity = 100.1250, DeliveredQuantity=0 }
                      };

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jvc_customerId, lsfo_productId, "1001",
                          new DateTime(2022, 8, 27), 100.1250, "Dialog",
                          OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge,
                          "", null, releaseEntries, new List<BowserEntryDto>());

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => orderSvc.CreateAsync(dto));

                      Assert.Equal("Product miss-matching entries: 1001", ex.Message);
                  });
            }

            [Fact]
            public async Task WhenThereAreInvalidEntryNos_ThrowsException()
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
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = new List<OrderStockReleaseEntryDto>
                      {
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="2001", ObRef="xyz", Quantity = 100.1250, DeliveredQuantity=0 }
                      };

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jvc_customerId, go_productId, "1001",
                          new DateTime(2022, 8, 27), 100.1250, "Dialog",
                          OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge,
                          "", null, releaseEntries, new List<BowserEntryDto>());

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => orderSvc.CreateAsync(dto));

                      Assert.Equal("Invalid entries: 2001", ex.Message);
                  });
            }

            [Fact]
            public async Task WhenThereAreMultipleInvalidEntryNos_ThrowsException()
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
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = new List<OrderStockReleaseEntryDto>
                      {
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="1001", ObRef="xyz", Quantity = 120, DeliveredQuantity=0 },
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="3001", ObRef="xyz", Quantity = 120, DeliveredQuantity=0 },
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="2001", ObRef="xyz", Quantity = 120, DeliveredQuantity=0 }
                      };

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jvc_customerId, go_productId, "1001",
                          new DateTime(2022, 8, 27), 360, "Dialog",
                          OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge,
                          "", null, releaseEntries, new List<BowserEntryDto>());

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => orderSvc.CreateAsync(dto));

                      Assert.Equal("Invalid entries: 2001|3001", ex.Message);
                  });
            }

            [Fact]
            public async Task WhenRequestedQuantitiesAreGreaterThanRemainingQuantity_ThrowsException()
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
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = new List<OrderStockReleaseEntryDto>
                      {
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="1001", ObRef="xyz", Quantity = 1500, DeliveredQuantity=0 },
                      };

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jvc_customerId, go_productId, "1001",
                          new DateTime(2022, 8, 27), 1500, "Dialog",
                          OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge,
                          "", null, releaseEntries, new List<BowserEntryDto>());

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => orderSvc.CreateAsync(dto));

                      Assert.Equal("Remaining quantity: 1000.25 of Entry: 1001 not sufficient to deliver requested quantity: 1500", ex.Message);
                  });
            }

            [Fact]
            public async Task WhenThereAreCompletedEntriesSelected_ThrowsException()
            {
                var jkcs_customerId = Guid.Empty;
                var lsfo_productId = Guid.Empty;

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      jkcs_customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JKCS");
                      lsfo_productId = await EntityHelper.GetProductIdAsync(dbContext, "380_LSFO");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = new List<OrderStockReleaseEntryDto>
                      {
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="1102", ObRef="abc", Quantity = 15, DeliveredQuantity=0 },
                      };

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jkcs_customerId, lsfo_productId, "2001",
                          new DateTime(2022, 8, 27), 15, "Mobitel",
                          OrderStatus.Undelivered, "OB-2", "100", BuyerType.Barge,
                          "", null, releaseEntries, new List<BowserEntryDto>());

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => orderSvc.CreateAsync(dto));

                      Assert.Equal("Completed entries: 1102 cannot be used", ex.Message);
                  });
            }

            [Fact]
            public async Task WhenThereAreMultipleIssuesWithEntries_ThrowsException()
            {
                var jkcs_customerId = Guid.Empty;
                var lsfo_productId = Guid.Empty;

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      jkcs_customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JKCS");
                      lsfo_productId = await EntityHelper.GetProductIdAsync(dbContext, "380_LSFO");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = new List<OrderStockReleaseEntryDto>
                      {
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="1001", ObRef="xyz", Quantity = 1500, DeliveredQuantity=0 },
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="3001", ObRef="xyz", Quantity = 120, DeliveredQuantity=0 },
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="2001", ObRef="xyz", Quantity = 120, DeliveredQuantity=0 }
                      };

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jkcs_customerId, lsfo_productId, "1001",
                          new DateTime(2022, 8, 27), 1740, "Dialog",
                          OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge,
                          "", null, releaseEntries, new List<BowserEntryDto>());

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => orderSvc.CreateAsync(dto));

                      Assert.Equal("Invalid entries: 2001|3001, Product miss-matching entries: 1001, Customer miss-matching entries: 1001, Remaining quantity: 1000.25 of Entry: 1001 not sufficient to deliver requested quantity: 1500", ex.Message);
                  });
            }


            [Fact]
            public async Task WhenPassingCorrectData_CreatedSuccessfully()
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
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = new List<OrderStockReleaseEntryDto>
                      {
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="1001", ObRef="xyz", Quantity = 120, DeliveredQuantity=120 }
                      };

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jvc_customerId, go_productId, "1",
                          new DateTime(2022, 8, 27), 120, "Dialog",
                          OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge,
                          "", "First order", releaseEntries, new List<BowserEntryDto>());

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
                       Assert.Equal("1", order.OrderNo);
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
                       Assert.Equal(-120, txn1.DeliveredQuantity);
                       Assert.Equal(EntryTransactionType.Out, txn1.Type);

                       //Entry
                       Assert.Equal(1000.250, entry.InitialQualtity);
                       Assert.Equal(880.250, entry.RemainingQuantity);
                       Assert.Equal(EntryStatus.Active, entry.Status);
                   });
            }

            [Fact]
            public async Task WhenEntryRemQtyIsZeroEntryMardAsCompleted_CreatedSuccessfully()
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
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = new List<OrderStockReleaseEntryDto>
                      {
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="1001", ObRef="xyz", Quantity = 1000.250, DeliveredQuantity=0 }
                      };

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jvc_customerId, go_productId, "1",
                          new DateTime(2022, 8, 27), 1000.250, "Dialog",
                          OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge,
                          "", "First order", releaseEntries, new List<BowserEntryDto>());

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
                       Assert.Equal("1", order.OrderNo);
                       Assert.Equal(1000.250, order.Quantity);
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
                       Assert.Equal(-1000.250, txn1.Quantity);
                       Assert.Equal(0, txn1.DeliveredQuantity);
                       Assert.Equal(EntryTransactionType.Out, txn1.Type);

                       //Entry
                       Assert.Equal(1000.250, entry.InitialQualtity);
                       Assert.Equal(0, entry.RemainingQuantity);
                       Assert.Equal(EntryStatus.Active, entry.Status);
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
                      id = await EntityHelper.GetOrderIdAsync(dbContext, "1501");
                      jvc_customerId = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                      lsfo_productId = await EntityHelper.GetProductIdAsync(dbContext, "380_LSFO");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var order = await orderSvc.GetOrderAsync(id);

                      //Order
                      Assert.NotNull(order);

                      Assert.Equal("1501", order.OrderNo);
                      Assert.Equal(199.5, order.Quantity);
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
                      Assert.Single(order.ReleaseEntries);

                      var txn1 = order.ReleaseEntries.First();

                      Assert.NotEqual(Guid.Empty, txn1.Id);
                      Assert.Equal("ref-10", txn1.ObRef);
                      Assert.Equal("1002", txn1.EntryNo);
                      Assert.Equal(200.250, txn1.Quantity);
                      Assert.Equal(199.500, txn1.DeliveredQuantity);

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

                      var order = orders.Items.First(o => o.OrderNo == "1501");

                      Assert.Equal("1501", order.OrderNo);
                      Assert.Equal(199.5, order.Quantity);
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
            var entryService = new EntryService(dataContext);
            var orderSvc = new OrderService(dataContext, entryService);
            return orderSvc;
        }
    }
}
