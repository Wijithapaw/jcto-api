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
            internal static async Task<List<Entry>> GetEntriesAsync(IDataContext dbContext, Guid customerId1, Guid productId1, Guid customerId2, Guid productId2, Guid orderId1, Guid orderId2, Guid orderId3)
            {
                var e1 = EntityHelper.CreateEntry("1001", customerId1, productId1, 1000.250m, 1000.250m, new DateTime(2022, 8, 20), EntryStatus.Active);
                var e1Txns = new List<EntryTransaction>
                {
                    EntityHelper.CreateEntryTransaction(EntryTransactionType.Approval, new DateTime(2022, 8, 21), string.Empty, 1000.250m, null, ApprovalType.Rebond, "50000"),
                };
                e1.Transactions = e1Txns;

                var e2 = EntityHelper.CreateEntry("1002", customerId1, productId2, 500, 220, new DateTime(2022, 8, 28), EntryStatus.Active);
                var e2Approval1 = EntityHelper.CreateEntryTransaction(EntryTransactionType.Approval, new DateTime(2022, 8, 28), string.Empty, 350, null, ApprovalType.XBond, "60000");
                var e2Txns = new List<EntryTransaction>
                {
                    e2Approval1,
                    EntityHelper.CreateEntryTransaction(EntryTransactionType.Out, new DateTime(2022, 8, 27), "ref-10", -189.500m, -180, ApprovalType.XBond, null, e2Approval1, orderId1),
                    EntityHelper.CreateEntryTransaction(EntryTransactionType.Out, new DateTime(2022, 8, 28), "ref-11", -100, null, ApprovalType.XBond, null, e2Approval1, orderId2)
                };
                e2.Transactions = e2Txns;

                var e3 = EntityHelper.CreateEntry("1101", customerId2, productId1, 750, 750, new DateTime(2022, 8, 20), EntryStatus.Active);
                var e3Txns = new List<EntryTransaction>
                {
                    EntityHelper.CreateEntryTransaction(EntryTransactionType.Approval, new DateTime(2022, 8, 20), string.Empty, 700, null, ApprovalType.Letter),
                };
                e3.Transactions = e3Txns;

                var e4 = EntityHelper.CreateEntry("1102", customerId2, productId2, 750, 150, new DateTime(2022, 8, 20), EntryStatus.Active);
                var e4Approval = EntityHelper.CreateEntryTransaction(EntryTransactionType.Approval, new DateTime(2022, 8, 20), string.Empty, 650, null, ApprovalType.Rebond, "50001");
                var e4Txns = new List<EntryTransaction>
                {
                    e4Approval,
                    EntityHelper.CreateEntryTransaction(EntryTransactionType.Out, new DateTime(2022, 8, 29), "ref-101", -500, null, ApprovalType.Rebond, null, e4Approval, orderId3),

                };
                e4.Transactions = e4Txns;

                var e5 = EntityHelper.CreateEntry("1103", customerId1, productId2, 200, 200, new DateTime(2022, 9, 9), EntryStatus.Active);
                var e5Txns = new List<EntryTransaction>
                {
                    EntityHelper.CreateEntryTransaction(EntryTransactionType.Approval, new DateTime(2022, 9, 10), string.Empty, 200, null, ApprovalType.Rebond, "15244"),
                };
                e5.Transactions = e5Txns;

                var e6 = EntityHelper.CreateEntry("1104", customerId1, productId2, 10, 0, new DateTime(2022, 9, 9), EntryStatus.Completed);
                var e6Approval = EntityHelper.CreateEntryTransaction(EntryTransactionType.Approval, new DateTime(2022, 9, 10), string.Empty, 10, null, ApprovalType.Rebond, "15344");
                var e6Txns = new List<EntryTransaction>
                {
                    e6Approval,
                    EntityHelper.CreateEntryTransaction(EntryTransactionType.Out, new DateTime(2022, 8, 27), "ref-21", -10, -10, ApprovalType.Rebond, null, e6Approval, orderId1),
                };
                e6.Transactions = e6Txns;

                var entries = new List<Entry> { e1, e2, e3, e4, e5, e6 };

                return entries;
            }
        }

        internal class Orders
        {
            internal static List<Order> GetOrders(Guid customerId1, Guid customerId2, Guid productId1, Guid productId2)
            {
                var orders = new List<Order>()
                {
                    EntityHelper.CreateOrder(customerId1, productId2, new DateTime(2022, 8, 27), 1501, "Exex", BuyerType.Bowser, "110", "OB/2022", OrderStatus.Delivered, 199.5m, 190, "Test 123", new List<BowserEntry> {EntityHelper.CreateBowserEntry(13600, 2)}),
                    EntityHelper.CreateOrder(customerId1, productId2, new DateTime(2022, 8, 28), 1502, "Samagi", BuyerType.Barge, "110", "OB/2022", OrderStatus.Undelivered, 100, null, "Test", new List<BowserEntry>()),
                    EntityHelper.CreateOrder(customerId2, productId2, new DateTime(2022, 8, 29), 1503, "Ins", BuyerType.Barge, "120", "OB/2022", OrderStatus.Undelivered, 500, null, "Test", new List<BowserEntry>())
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

                var orderId_1501 = await EntityHelper.GetOrderIdAsync(dbContext, 1501);
                var orderId_1502 = await EntityHelper.GetOrderIdAsync(dbContext, 1502);
                var orderId_1503 = await EntityHelper.GetOrderIdAsync(dbContext, 1503);

                dbContext.Entries.AddRange(await Entries.GetEntriesAsync(dbContext, customerId1, productId1, customerId2, productId2, orderId_1501, orderId_1502, orderId_1503));
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
