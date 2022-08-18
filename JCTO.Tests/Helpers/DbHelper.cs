using JCTO.Data;
using JCTO.Domain;
using JCTO.Domain.Dtos;
using JCTO.Domain.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCTO.Tests.Helpers
{
    internal class DbHelper
    {
        public const string UNIT_TEST_USER_ID = "e7240cc5-760d-4261-a85b-15150dfcee05";
        private static SqliteConnection GetSqliteConnection()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            return connection;
        }

        private static DbContextOptions<DataContext> GetSqliteContextOptions(SqliteConnection connection)
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                   .UseSqlite(connection)
                   .Options;

            using (var context = new DataContext(options, GetUserContext()))
            {
                context.Database.EnsureCreated();
            }

            return options;
        }

        public static IUserContext GetUserContext() => new UserContext { UserId = UNIT_TEST_USER_ID };

        private static async Task CreateUnitTestUserAsync(IDataContext dbContext)
        {
            dbContext.Users.Add(new User
            {
                Id = Guid.Parse(UNIT_TEST_USER_ID),
                Email = "unittestuser@yopmail.com",
                FirstName = "Unit",
                LastName = "Test"
            });
            await dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// This is a helper method to execute the 3 stages (Setting up test data, Test execution, Additinal validations) of a test method
        /// </summary>
        /// <param name="funcSetupTestDataAsync(IDataContext context)"> Set up test data inside this method. </param>
        /// <param name="funcTextExecutionAsync(IDataContext context)"> Place the main tst logic inside here. </param>
        /// <param name="funcValidationsAsync(IDataContext context)"> Use this method for additional validations against a fresh data context. This is optional. </param>
        /// <returns></returns>
        public static async Task ExecuteTestAsync(Func<IDataContext, Task> funcSetupTestDataAsync,
            Func<IDataContext, Task> funcTextExecutionAsync,
            Func<IDataContext, Task>? funcValidationsAsync = null)
        {
            using (var connection = GetSqliteConnection())
            {
                var options = GetSqliteContextOptions(connection);

                if (funcSetupTestDataAsync != null)
                {
                    using (IDataContext context = new DataContext(options, GetUserContext()))
                    {
                        await CreateUnitTestUserAsync(context);

                        await funcSetupTestDataAsync(context);
                    }
                }

                using (IDataContext context = new DataContext(options, GetUserContext()))
                {
                    await funcTextExecutionAsync(context);
                }

                if (funcValidationsAsync != null)
                {
                    using (IDataContext context = new DataContext(options, GetUserContext()))
                    {
                        await funcValidationsAsync(context);
                    }
                }
            }
        }
    }
}
