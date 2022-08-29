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

        public class GetProductListItems
        {
            [Fact]
            public async Task WhenProductsExists_ReturnAllActive()
            {
                await DbHelper.ExecuteTestAsync(
                   async (IDataContext dbContext) =>
                   {
                       dbContext.Products.AddRange(TestData.Products.GetProducts());
                       await dbContext.SaveChangesAsync();
                   },
                   async (IDataContext dbContext) =>
                   {
                       var customerSvc = new CustomerService(dbContext);

                       var products = await customerSvc.GetProductListItemsAsync();

                       Assert.Equal(3, products.Count);

                       Assert.Contains(products, p => p.Label == "GO");
                       Assert.Contains(products, p => p.Label == "380_LSFO");
                       Assert.Contains(products, p => p.Label == "380_HSFO");
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
