using JCTO.Domain;
using JCTO.Services;
using JCTO.Tests.Helpers;

namespace JCTO.Tests
{
    public class CustomerServiceTests
    {
        public class GetAllCustomersListItems
        {
            [Fact]
            public async Task WhenCustomersExists_ReturnAllAsListItems()
            {
                await DbHelper.ExecuteTestAsync(
                   async (IDataContext dbContext) =>
                   {
                       await SetupTestDataAsync(dbContext);
                   },
                   async (IDataContext dbContext) =>
                   {
                       var customerSvc = new CustomerService(dbContext);

                       var customers = await customerSvc.GetAllCustomersListItemsAsync();

                       Assert.Equal(3, customers.Count);
                       Assert.DoesNotContain(customers, c => c.Label == "EXPO");
                   });
            }
        }

        public class GetAllCustomerStocks
        {
            [Fact]
            public async Task WhenCustomersExists_ReturnAllStocks()
            {
                await DbHelper.ExecuteTestAsync(
                   async (IDataContext dbContext) =>
                   {
                       await SetupTestDataAsync(dbContext);
                   },
                   async (IDataContext dbContext) =>
                   {
                       var customerSvc = new CustomerService(dbContext);

                       var customerStocks = await customerSvc.GetAllCustomerStocksAsync();

                       Assert.Equal(3, customerStocks.Count);

                       //TODO: Add more assertions when real stocks are there.
                   });
            }
        }

        private static async Task SetupTestDataAsync(IDataContext dbContext)
        {
            dbContext.Customers.AddRange(TestData.Customers.GetCustomers());

            await dbContext.SaveChangesAsync();
        }
    }
}
