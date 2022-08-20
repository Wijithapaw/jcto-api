using JCTO.Domain.Dtos;

namespace JCTO.Domain.Services
{
    public interface IEntryService
    {
        Task<EntityCreateResult> CreateAsync(EntryDto dto);
    }
}
