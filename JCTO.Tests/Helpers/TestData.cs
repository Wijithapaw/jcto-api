using JCTO.Domain;
using JCTO.Domain.Entities;
using JCTO.Domain.Enums;

namespace JCTO.Tests.Helpers
{
    internal static class TestData
    {
        internal static class Users
        {
            internal static User[] GetUsers()
            {
                var users = new User[]
                {
                    EntityHelper.CreateUser("Kusal", "Mendis", "kusal@yopmail.com"),
                    EntityHelper.CreateUser("Amal", "Perera", "amal@yopmail.com"),
                    EntityHelper.CreateUser("Kamal", "Perera", "kamal@yopmail.com"),
                };
                return users;
            }
        }

        internal static class Customers
        {
            internal static Customer[] GetCustomers()
            {
                var customers = new Customer[]
                {
                    EntityHelper.CreateCustomer("JVC"),
                    EntityHelper.CreateCustomer("JKCS"),
                    EntityHelper.CreateCustomer("Mobil"),
                    EntityHelper.CreateCustomer("EXPO", true),
                };

                return customers;
            }
        }

        internal static class Products
        {
            internal static Product[] GetProducts()
            {
                var products = new Product[]
                {
                    EntityHelper.CreateProduct("GO"),
                    EntityHelper.CreateProduct("380_LSFO"),
                    EntityHelper.CreateProduct("380_HSFO"),
                    EntityHelper.CreateProduct("380_HSFO_OLD", true),
                };

                return products;
            }
        }

        internal static class Entries
        {
            internal static List<Entry> GetEntries(Guid customerId1, Guid productId1, Guid customerId2, Guid productId2, Guid orderId1, Guid orderId2, Guid orderId3)
            {
                var e1 = EntityHelper.CreateEntry("1001", customerId1, productId1, 1000.250, 1000.250, new DateTime(2022, 8, 20), EntryStatus.Active);
                var e1Txns = new List<EntryTransaction>
                {
                    EntityHelper.CreateEntryTransaction(EntryTransactionType.In, string.Empty, 1000.250, 1000.250),
                };
                e1.Transactions = e1Txns;

                var e2 = EntityHelper.CreateEntry("1002", customerId1, productId2, 500, 205.5, new DateTime(2022, 8, 28), EntryStatus.Active);
                var e2Txns = new List<EntryTransaction>
                {
                    EntityHelper.CreateEntryTransaction(EntryTransactionType.In, string.Empty, 500, 500),
                    EntityHelper.CreateEntryTransaction(EntryTransactionType.Out, "ref-10", -200.250, -199.500, orderId1),
                    EntityHelper.CreateEntryTransaction(EntryTransactionType.Out, "ref-11", -100, -95, orderId2)
                };
                e2.Transactions = e2Txns;

                var e3 = EntityHelper.CreateEntry("1101", customerId2, productId1, 750, 750, new DateTime(2022, 8, 20), EntryStatus.Active);
                var e3Txns = new List<EntryTransaction>
                {
                    EntityHelper.CreateEntryTransaction(EntryTransactionType.In, string.Empty, 750, 750),
                };
                e3.Transactions = e3Txns;

                var e4 = EntityHelper.CreateEntry("1102", customerId2, productId2, 750, 0, new DateTime(2022, 8, 20), EntryStatus.Completed);
                var e4Txns = new List<EntryTransaction>
                {
                    EntityHelper.CreateEntryTransaction(EntryTransactionType.In, string.Empty, 750, 750),
                    EntityHelper.CreateEntryTransaction(EntryTransactionType.Out, "ref-101", -500, -500, orderId3),
                    EntityHelper.CreateEntryTransaction(EntryTransactionType.Out, "ref-112", -300, -250, orderId2)
                };
                e4.Transactions = e4Txns;

                var entries = new List<Entry> { e1, e2, e3, e4 };

                return entries;
            }
        }

        internal class Orders
        {
            internal static List<Order> GetOrders(Guid customerId1, Guid customerId2, Guid productId1, Guid productId2)
            {
                var orders = new List<Order>()
                {
                    EntityHelper.CreateOrder(customerId1, productId2, new DateTime(2022, 8, 27), "1501", "Exex", BuyerType.Bowser, "110", "OB/2022", OrderStatus.Delivered, 199.5, "Test 123", new List<BowserEntry> {EntityHelper.CreateBowserEntry(13600, 2)}),
                    EntityHelper.CreateOrder(customerId1, productId2, new DateTime(2022, 8, 28), "1502", "Samagi", BuyerType.Barge, "110", "OB/2022", OrderStatus.Undelivered, 345, "Test", new List<BowserEntry>()),
                    EntityHelper.CreateOrder(customerId2, productId2, new DateTime(2022, 8, 29), "1503", "Ins", BuyerType.Barge, "120", "OB/2022", OrderStatus.Undelivered, 500, "Test", new List<BowserEntry>())
                };

                return orders;
            }

            internal static async Task SetupOrderAndEntryTestDataAsync(IDataContext dbContext)
            {
                dbContext.Customers.AddRange(Customers.GetCustomers());
                dbContext.Products.AddRange(Products.GetProducts());
                await dbContext.SaveChangesAsync();

                var customerId1 = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                var customerId2 = await EntityHelper.GetCustomerIdAsync(dbContext, "JKCS");
                var productId1 = await EntityHelper.GetProductIdAsync(dbContext, "GO");
                var productId2 = await EntityHelper.GetProductIdAsync(dbContext, "380_LSFO");

                dbContext.Orders.AddRange(GetOrders(customerId1, customerId2, productId1, productId2));
                await dbContext.SaveChangesAsync();

                var orderId_1501 = await EntityHelper.GetOrderIdAsync(dbContext, "1501");
                var orderId_1502 = await EntityHelper.GetOrderIdAsync(dbContext, "1502");
                var orderId_1503 = await EntityHelper.GetOrderIdAsync(dbContext, "1503");

                dbContext.Entries.AddRange(Entries.GetEntries(customerId1, productId1, customerId2, productId2, orderId_1501, orderId_1502, orderId_1503));
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
