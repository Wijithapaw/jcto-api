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
                Guid id = Guid.Empty;
                var customerId = Guid.Empty;
                var productId = Guid.Empty;

                var releaseEntries = new List<OrderStockReleaseEntry>
                {
                    new OrderStockReleaseEntry { Id=Guid.NewGuid(), EntryNo="10001", ObRef="xyz", Quantity = 110, DeliveredQuantity=100.1250 }
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
                      var orderSvc = new OrderService(dbContext);

                      var dto = DtoHelper.CreateOrderDto(Guid.Empty, customerId, productId, orderNo, orderDate, quantity,
                          buyer, status, obPrefix, tankNo, buyerType, xBondNo, remarks, releaseEntries, new List<BowserEntry>());

                      var ex = await Assert.ThrowsAsync<JCTOValidationException>(() => orderSvc.CreateAsync(dto));

                      Assert.Equal(expectedError, ex.Message);
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
