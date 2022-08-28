using JCTO.Domain.Dtos;
using JCTO.Domain.Dtos.Base;

namespace JCTO.Domain.Services
{
    public interface IOrderService
    {
        Task<EntityCreateResult> CreateAsync(OrderDto dto);
        Task<PagedResultsDto<OrderListItemDto>> SearchOrdersAsync(OrderSearchDto filter);
        Task<OrderDto> GetOrderAsync(Guid id);
    }
}
