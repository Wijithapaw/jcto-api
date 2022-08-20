using JCTO.Domain.Entities;
using JCTO.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            internal static List<Entry> GetEntries(Guid customerId, Guid productId)
            {
                var e1 = EntityHelper.CreateEntry("1001", customerId, productId, 1000.250, new DateTime(2022, 8, 20), EntryStatus.Active);
                var e1Txns = new List<EntryTransaction>
                {
                    EntityHelper.CreateEntryTransaction(EntryTransactionType.In, 1000.250),
                    EntityHelper.CreateEntryTransaction(EntryTransactionType.In, 1000.250)
                };
                e1.Transactions = e1Txns;

                var entries = new List<Entry> { e1 };

                return entries;
            }
        }
    }
}
