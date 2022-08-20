using JCTO.Domain;
using JCTO.Domain.CustomExceptions;
using JCTO.Services;
using JCTO.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace JCTO.Tests
{
    public class UserServiceTests
    {
        public class GetAll
        {
            [Fact]
            public async Task WhenUsersExists_ReturnAll()
            {
                await DbHelper.ExecuteTestAsync(
                   async (IDataContext dbContext) =>
                   {
                       await SetupTestDataAsync(dbContext);
                   },
                   async (IDataContext dbContext) =>
                   {
                       var userSvc = new UserService(dbContext);

                       var users = await userSvc.GetAllAsync();

                       Assert.NotEmpty(users);
                   });
            }            
        }

        public class Update
        {
            [Fact]
            public async Task WhenUsersExists_UpdatesSuccessfully()
            {
                await DbHelper.ExecuteTestAsync(
                   async (IDataContext dbContext) =>
                   {
                       await SetupTestDataAsync(dbContext);
                   },
                   async (IDataContext dbContext) =>
                   {
                       var userSvc = new UserService(dbContext);

                       var user = await dbContext.Users.FirstAsync(u => u.Email == "kusal@yopmail.com");

                       var oldConcurrencyKey = user.ConcurrencyKey;

                       var userDto = DtoHelper.CreateUserDto("Kusal New", "Mendis", "kusal@yopmail.com", oldConcurrencyKey);
                       var result = await userSvc.UpdateAsync(user.Id, userDto);

                       Assert.NotEqual(result.ConcurrencyKey, oldConcurrencyKey);
                   });
            }

            [Fact]
            public async Task WhenThereIsConcurencyViolation_ThrowsException()
            {
                await DbHelper.ExecuteTestAsync(
                   async (IDataContext dbContext) =>
                   {
                       await SetupTestDataAsync(dbContext);
                   },
                   async (IDataContext dbContext) =>
                   {
                       var userSvc = new UserService(dbContext);

                       var user = await dbContext.Users.FirstAsync(u => u.Email == "kusal@yopmail.com");

                       var oldConcurrencyKey = user.ConcurrencyKey;

                       var userDto = DtoHelper.CreateUserDto("Kusal New", "Mendis", "kusal@yopmail.com", oldConcurrencyKey);
                       var result = await userSvc.UpdateAsync(user.Id, userDto);

                       Assert.NotEqual(result.ConcurrencyKey, oldConcurrencyKey);

                       userDto.FirstName = "Kusal New 2";
                       var ex = await Assert.ThrowsAsync<JCTOConcurrencyException>(() => userSvc.UpdateAsync(user.Id, userDto));

                       Assert.Equal("Concurrency violation of Entity: User", ex.Message);
                   });
            }
        }

        private static async Task SetupTestDataAsync(IDataContext dbContext)
        {
            dbContext.Users.AddRange(TestData.Users.GetUsers());

            await dbContext.SaveChangesAsync();
        }
    }
}