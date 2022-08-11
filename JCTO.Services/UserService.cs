using JCTO.Domain;
using JCTO.Domain.CustomExceptions;
using JCTO.Domain.Dtos;
using JCTO.Domain.Entities;
using JCTO.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace JCTO.Services
{
    public class UserService : IUserService
    {
        private readonly IJctoDbContext _dbContext;

        public UserService(IJctoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<UserDto>> GetAllAsync()
        {
            var users = await _dbContext.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                }).ToListAsync();

            return users;
        }

        public async Task<UserDto?> GetAsync(Guid id)
        {
            var user = await _dbContext.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    ConcurrencyKey = u.ConcurrencyKey
                }).FirstOrDefaultAsync();

            return user;
        }

        public async Task<EntityCreateResult> RegisterAsync(UserDto userDto)
        {
            var user = new User
            {
                //Id = userDto.Id,
                Email = userDto.Email,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
            };

            _dbContext.Users.Add(user);

            await _dbContext.SaveChangesAsync();

            return new EntityCreateResult { Id = user.Id!.Value, ConcurrencyKey = user.ConcurrencyKey!.Value };
        }

        public async Task<EntityUpdateResult> UpdateAsync(Guid id, UserDto userDto)
        {
            var user = await _dbContext.Users.FindAsync(id);

            if (user == null)
                throw new JCTOException($"User not found: Id: {id}");

            user.Email = userDto.Email;
            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.ConcurrencyKey = userDto.ConcurrencyKey;

            await _dbContext.SaveChangesAsync();

            return new EntityUpdateResult { ConcurrencyKey = user.ConcurrencyKey!.Value };
        }
    }
}