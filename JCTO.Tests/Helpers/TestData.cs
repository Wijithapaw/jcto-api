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
                var dischargeTxn_501 = await EntityHelper.GetStockTxnAsync(dbContext, "501");
                var e1StockTxn = EntityHelper.CreateStockTransaction(dischargeTxn_501.Stock.Id, 1000.250, new DateTime(2022, 8, 20), StockTransactionType.Out, null, dischargeTxn_501);
                var e1 = EntityHelper.CreateEntry("1001", customerId1, productId1, 1000.250, 1000.250, new DateTime(2022, 8, 20), EntryStatus.Active, e1StockTxn);
                var e1Txns = new List<EntryTransaction>
                {
                    EntityHelper.CreateEntryTransaction(EntryTransactionType.Approval, new DateTime(2022, 8, 21), string.Empty, 1000.250, null, ApprovalType.Rebond, "50000"),
                };
                e1.Transactions = e1Txns;

                var dischargeTxn_520 = await EntityHelper.GetStockTxnAsync(dbContext, "520");
                var e2StockTxn = EntityHelper.CreateStockTransaction(dischargeTxn_520.Stock.Id, 500, new DateTime(2022, 8, 20), StockTransactionType.Out, null, dischargeTxn_520);
                var e2 = EntityHelper.CreateEntry("1002", customerId1, productId2, 500, 220, new DateTime(2022, 8, 28), EntryStatus.Active, e2StockTxn);
                var e2Approval1 = EntityHelper.CreateEntryTransaction(EntryTransactionType.Approval, new DateTime(2022, 8, 28), string.Empty, 350, null, ApprovalType.XBond, "60000");
                var e2Txns = new List<EntryTransaction>
                {
                    e2Approval1,
                    EntityHelper.CreateEntryTransaction(EntryTransactionType.Out, new DateTime(2022, 8, 27), "ref-10", -189.500, -180, ApprovalType.XBond, null, e2Approval1, orderId1),
                    EntityHelper.CreateEntryTransaction(EntryTransactionType.Out, new DateTime(2022, 8, 28), "ref-11", -100, null, ApprovalType.XBond, null, e2Approval1, orderId2)
                };
                e2.Transactions = e2Txns;

                var dischargeTxn_513 = await EntityHelper.GetStockTxnAsync(dbContext, "513");
                var e3StockTxn = EntityHelper.CreateStockTransaction(dischargeTxn_513.Stock.Id, 750, new DateTime(2022, 8, 20), StockTransactionType.Out, null, dischargeTxn_513);
                var e3 = EntityHelper.CreateEntry("1101", customerId2, productId1, 750, 750, new DateTime(2022, 8, 20), EntryStatus.Active, e3StockTxn);
                var e3Txns = new List<EntryTransaction>
                {
                    EntityHelper.CreateEntryTransaction(EntryTransactionType.Approval, new DateTime(2022, 8, 20), string.Empty, 700, null, ApprovalType.Letter),
                };
                e3.Transactions = e3Txns;

                var dischargeTxn_540 = await EntityHelper.GetStockTxnAsync(dbContext, "540");
                var e4StockTxn = EntityHelper.CreateStockTransaction(dischargeTxn_540.Stock.Id, 750, new DateTime(2022, 8, 20), StockTransactionType.Out, null, dischargeTxn_540);
                var e4 = EntityHelper.CreateEntry("1102", customerId2, productId2, 750, 150, new DateTime(2022, 8, 20), EntryStatus.Active, e4StockTxn);
                var e4Approval = EntityHelper.CreateEntryTransaction(EntryTransactionType.Approval, new DateTime(2022, 8, 20), string.Empty, 650, null, ApprovalType.Rebond, "50001");
                var e4Txns = new List<EntryTransaction>
                {
                    e4Approval,
                    EntityHelper.CreateEntryTransaction(EntryTransactionType.Out, new DateTime(2022, 8, 29), "ref-101", -500, null, ApprovalType.Rebond, null, e4Approval, orderId3),

                };
                e4.Transactions = e4Txns;

                var e5StockTxn = EntityHelper.CreateStockTransaction(dischargeTxn_520.Stock.Id, 200, new DateTime(2022, 8, 20), StockTransactionType.Out, null, dischargeTxn_520);
                var e5 = EntityHelper.CreateEntry("1103", customerId1, productId2, 200, 200, new DateTime(2022, 9, 9), EntryStatus.Active, e5StockTxn);
                var e5Txns = new List<EntryTransaction>
                {
                    EntityHelper.CreateEntryTransaction(EntryTransactionType.Approval, new DateTime(2022, 9, 10), string.Empty, 200, null, ApprovalType.Rebond, "15244"),
                };
                e5.Transactions = e5Txns;

                var e6StockTxn = EntityHelper.CreateStockTransaction(dischargeTxn_520.Stock.Id, 10, new DateTime(2022, 8, 20), StockTransactionType.Out, null, dischargeTxn_520);
                var e6 = EntityHelper.CreateEntry("1104", customerId1, productId2, 10, 0, new DateTime(2022, 9, 9), EntryStatus.Completed, e6StockTxn);
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
                    EntityHelper.CreateOrder(customerId1, productId2, new DateTime(2022, 8, 27), "1501", "Exex", BuyerType.Bowser, "110", "OB/2022", OrderStatus.Delivered, 199.5, 190, "Test 123", new List<BowserEntry> {EntityHelper.CreateBowserEntry(13600, 2)}),
                    EntityHelper.CreateOrder(customerId1, productId2, new DateTime(2022, 8, 28), "1502", "Samagi", BuyerType.Barge, "110", "OB/2022", OrderStatus.Undelivered, 100, null, "Test", new List<BowserEntry>()),
                    EntityHelper.CreateOrder(customerId2, productId2, new DateTime(2022, 8, 29), "1503", "Ins", BuyerType.Barge, "120", "OB/2022", OrderStatus.Undelivered, 500, null, "Test", new List<BowserEntry>())
                };

                return orders;
            }

            internal static async Task SetupOrderAndEntryTestDataAsync(IDataContext dbContext)
            {
                await Stocks.CreateStockAsync(dbContext);

                var customerId1 = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                var customerId2 = await EntityHelper.GetCustomerIdAsync(dbContext, "JKCS");
                var productId1 = await EntityHelper.GetProductIdAsync(dbContext, "GO");
                var productId2 = await EntityHelper.GetProductIdAsync(dbContext, "380_LSFO");

                dbContext.Orders.AddRange(GetOrders(customerId1, customerId2, productId1, productId2));
                await dbContext.SaveChangesAsync();

                var orderId_1501 = await EntityHelper.GetOrderIdAsync(dbContext, "1501");
                var orderId_1502 = await EntityHelper.GetOrderIdAsync(dbContext, "1502");
                var orderId_1503 = await EntityHelper.GetOrderIdAsync(dbContext, "1503");

                dbContext.Entries.AddRange(await Entries.GetEntriesAsync(dbContext, customerId1, productId1, customerId2, productId2, orderId_1501, orderId_1502, orderId_1503));
                await dbContext.SaveChangesAsync();
            }
        }

        internal class Stocks
        {
            internal static async Task CreateStockAsync(IDataContext dbContext)
            {
                dbContext.Customers.AddRange(Customers.GetCustomers());
                dbContext.Products.AddRange(Products.GetProducts());
                await dbContext.SaveChangesAsync();

                var customerId1 = await EntityHelper.GetCustomerIdAsync(dbContext, "JVC");
                var customerId2 = await EntityHelper.GetCustomerIdAsync(dbContext, "JKCS");
                var productId1 = await EntityHelper.GetProductIdAsync(dbContext, "GO");
                var productId2 = await EntityHelper.GetProductIdAsync(dbContext, "380_LSFO");

                var s1 = EntityHelper.CreateStock(customerId1, productId1, 1080);
                var s1_dis = new List<StockTransaction>
                {
                    EntityHelper.CreateStockTransaction(Guid.Empty, 1000, new DateTime(2022, 9, 1), StockTransactionType.In, "501"),
                    EntityHelper.CreateStockTransaction(Guid.Empty, 50, new DateTime(2022, 9, 2), StockTransactionType.In, "502"),
                    EntityHelper.CreateStockTransaction(Guid.Empty, 30, new DateTime(2022, 9, 3), StockTransactionType.In, "503")
                };
                s1.Transactions = s1_dis;


                var s2 = EntityHelper.CreateStock(customerId1, productId2, 5000);
                var s2_dis = new List<StockTransaction>
                {
                    EntityHelper.CreateStockTransaction(Guid.Empty, 5000, new DateTime(2022, 9, 1), StockTransactionType.In, "520"),
                };
                s2.Transactions = s2_dis;


                var s3 = EntityHelper.CreateStock(customerId2, productId1, 1100);
                var s3_dis = new List<StockTransaction>
                {
                    EntityHelper.CreateStockTransaction(Guid.Empty, 20, new DateTime(2022, 9, 1), StockTransactionType.In, "510"),
                    EntityHelper.CreateStockTransaction(Guid.Empty, 50, new DateTime(2022, 9, 2), StockTransactionType.In, "511"),
                    EntityHelper.CreateStockTransaction(Guid.Empty, 30, new DateTime(2022, 9, 3), StockTransactionType.In, "512"),
                    EntityHelper.CreateStockTransaction(Guid.Empty, 1000, new DateTime(2022, 9, 6), StockTransactionType.In, "513")
                };
                s3.Transactions = s3_dis;

                var s4 = EntityHelper.CreateStock(customerId2, productId2, 2000);
                var s4_dis = new List<StockTransaction>
                {
                    EntityHelper.CreateStockTransaction(Guid.Empty, 2000, new DateTime(2022, 9, 1), StockTransactionType.In, "540"),
                };
                s4.Transactions = s4_dis;

                dbContext.Stocks.AddRange(new List<Stock> { s1, s2, s3, s4 });
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
