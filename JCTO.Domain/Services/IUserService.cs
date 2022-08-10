using JCTO.Domain.Dtos;

namespace JCTO.Domain.Services
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllAsync();
        Task<UserDto?> GetAsync(Guid id);
        Task<EntityUpdateResult> UpdateAsync(Guid id, UserDto userDto);
    }
}
