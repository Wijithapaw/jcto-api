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
            [InlineData("1001", "2022-8-26", 111, "Dialog", OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge, "", null, "Sum of Delivered Quantities not equal to overall Quantity")]
            [InlineData("1001", null, 100.125, "Dialog", OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge, "", null, "Order Date not found")]
            [InlineData("", "2022-8-26", 100.125, "Dialog", OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge, "", null, "Order No. not found")]
            [InlineData("1001", "2022-8-26", 100.125, "", OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge, "", null, "Buyer not found")]
            [InlineData("1001", "2022-8-26", 100.125, "Dialog", OrderStatus.Undelivered, "", "", BuyerType.Barge, "", null, "OBRef not found, Tank No. not found")]
            [InlineData("1001", "2022-8-26", 0, "Dialog", OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge, "", null, "Quantity must be > 0, Sum of Delivered Quantities not equal to overall Quantity")]
            [InlineData("1001", "2022-8-26", 0, "", OrderStatus.Undelivered, "", "", BuyerType.Barge, "", null, "Buyer not found, Quantity must be > 0, OBRef not found, Tank No. not found, Sum of Delivered Quantities not equal to overall Quantity")]
            public async Task WhenPassingValidData_CreateSuccessfully(string orderNo, DateTime orderDate, double quantity,
            string buyer, OrderStatus status, string obPrefix, string tankNo, BuyerType buyerType,
            string xBondNo, string remarks, string expectedError)
            {
                var customerId = Guid.Empty;
                var productId = Guid.Empty;

                var releaseEntries = new List<OrderStockReleaseEntryDto>
                {
                    new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="10001", ObRef="xyz", Quantity = 110, DeliveredQuantity=100.1250 }
                };

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      customerId = await GetCustomerIdAsync(dbContext, "JVC");
                      productId = await GetProductIdAsync(dbContext, "GO");
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

                      jkcs_customerId = await GetCustomerIdAsync(dbContext, "JKCS");
                      go_productId = await GetProductIdAsync(dbContext, "GO");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = new List<OrderStockReleaseEntryDto>
                      {
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="1001", ObRef="xyz", Quantity = 120, DeliveredQuantity=100.1250 }
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

                      jvc_customerId = await GetCustomerIdAsync(dbContext, "JVC");
                      lsfo_productId = await GetProductIdAsync(dbContext, "380_LSFO");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = new List<OrderStockReleaseEntryDto>
                      {
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="1001", ObRef="xyz", Quantity = 120, DeliveredQuantity=100.1250 }
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

                      jvc_customerId = await GetCustomerIdAsync(dbContext, "JVC");
                      go_productId = await GetProductIdAsync(dbContext, "GO");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = new List<OrderStockReleaseEntryDto>
                      {
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="2001", ObRef="xyz", Quantity = 120, DeliveredQuantity=100.1250 }
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

                      jvc_customerId = await GetCustomerIdAsync(dbContext, "JVC");
                      go_productId = await GetProductIdAsync(dbContext, "GO");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = new List<OrderStockReleaseEntryDto>
                      {
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="1001", ObRef="xyz", Quantity = 120, DeliveredQuantity=100 },
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="3001", ObRef="xyz", Quantity = 120, DeliveredQuantity=100 },
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="2001", ObRef="xyz", Quantity = 120, DeliveredQuantity=100 }
                      };

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jvc_customerId, go_productId, "1001",
                          new DateTime(2022, 8, 27), 300, "Dialog",
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

                      jvc_customerId = await GetCustomerIdAsync(dbContext, "JVC");
                      go_productId = await GetProductIdAsync(dbContext, "GO");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = new List<OrderStockReleaseEntryDto>
                      {
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="1001", ObRef="xyz", Quantity = 1500, DeliveredQuantity=1200 },
                      };

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jvc_customerId, go_productId, "1001",
                          new DateTime(2022, 8, 27), 1200, "Dialog",
                          OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge,
                          "", null, releaseEntries, new List<BowserEntryDto>());

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => orderSvc.CreateAsync(dto));

                      Assert.Equal("Remaining quantity: 1000.25 of Entry: 1001 not sufficient to deliver requested quantity: 1200", ex.Message);
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

                      jkcs_customerId = await GetCustomerIdAsync(dbContext, "JKCS");
                      lsfo_productId = await GetProductIdAsync(dbContext, "380_LSFO");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = new List<OrderStockReleaseEntryDto>
                      {
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="1001", ObRef="xyz", Quantity = 1500, DeliveredQuantity=1200 },
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="3001", ObRef="xyz", Quantity = 120, DeliveredQuantity=100 },
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="2001", ObRef="xyz", Quantity = 120, DeliveredQuantity=100 }
                      };

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jkcs_customerId, lsfo_productId, "1001",
                          new DateTime(2022, 8, 27), 1400, "Dialog",
                          OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge,
                          "", null, releaseEntries, new List<BowserEntryDto>());

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => orderSvc.CreateAsync(dto));

                      Assert.Equal("Invalid entries: 2001|3001, Product miss-matching entries: 1001, Customer miss-matching entries: 1001, Remaining quantity: 1000.25 of Entry: 1001 not sufficient to deliver requested quantity: 1200", ex.Message);
                  });
            }

            [Fact]
            public async Task WhenPassingCorrectData_CreatedSuccessfully()
            {
                var jvc_customerId = Guid.Empty;
                var go_productId = Guid.Empty;

                await DbHelper.ExecuteTestAsync(
                  async (IDataContext dbContext) =>
                  {
                      await SetupTestDataAsync(dbContext);

                      jvc_customerId = await GetCustomerIdAsync(dbContext, "JVC");
                      go_productId = await GetProductIdAsync(dbContext, "GO");
                  },
                  async (IDataContext dbContext) =>
                  {
                      var orderSvc = CreateService(dbContext);

                      var releaseEntries = new List<OrderStockReleaseEntryDto>
                      {
                          new OrderStockReleaseEntryDto { Id=Guid.NewGuid(), EntryNo="1001", ObRef="xyz", Quantity = 120, DeliveredQuantity=120 }
                      };

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, jvc_customerId, go_productId, "1001",
                          new DateTime(2022, 8, 27), 120, "Dialog",
                          OrderStatus.Undelivered, "OB-1", "100", BuyerType.Barge,
                          "", null, releaseEntries, new List<BowserEntryDto>());

                      var result = await orderSvc.CreateAsync(dto);

                      Assert.NotNull(result.Id);
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

        private static OrderService CreateService(IDataContext dataContext)
        {
            var entryService = new EntryService(dataContext);
            var orderSvc = new OrderService(dataContext, entryService);
            return orderSvc;
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
