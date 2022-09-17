using JCTO.Domain.Dtos;
using JCTO.Domain.Dtos.Base;

namespace JCTO.Domain.Services
{
    public interface IOrderService
    {
        Task<EntityCreateResult> CreateAsync(OrderDto dto);
        Task<EntityUpdateResult> UpdateAsync(Guid id, OrderDto dto);
        Task DeleteAsync(Guid id);
        Task<PagedResultsDto<OrderListItemDto>> SearchOrdersAsync(OrderSearchDto filter);
        Task<OrderDto> GetOrderAsync(Guid id);
        Task<byte[]> GenerateStockReleaseAsync(Guid orderId);
        Task<int> GetNextOrderNoAsync(DateTime date);
    }
}
