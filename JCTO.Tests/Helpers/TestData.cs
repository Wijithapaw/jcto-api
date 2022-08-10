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
    }
}
