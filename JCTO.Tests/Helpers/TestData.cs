using JCTO.Domain.Entities;
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
    }
}
