using JCTO.Domain.Dtos;

namespace JCTO.Domain.Services
{
    public interface IOrderService
    {
        Task<EntityCreateResult> CreateAsync(OrderDto dto);
    }
}
