using JCTO.Domain.Constants;
using JCTO.Domain.Dtos;
using JCTO.Domain.Dtos.Base;
using JCTO.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SelectPdf;
using System.Reflection;
using System.Text;

namespace JCTO.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<EntityCreateResult> Create(OrderDto dto)
        {
            return await _orderService.CreateAsync(dto);
        }

        [HttpPut("{id}")]
        public async Task<EntityUpdateResult> Update(Guid id, OrderDto dto)
        {
            return await _orderService.UpdateAsync(id, dto);
        }

        [HttpDelete("{id}")]
        public async Task Delete(Guid id)
        {
            await _orderService.DeleteAsync(id);
        }

        [HttpPut("{id}/Cancel")]
        public async Task Cancel(Guid id)
        {
            await _orderService.CancelOrderAsync(id);
        }

        [HttpGet("{id}")]
        public async Task<OrderDto> Get(Guid id)
        {
            return await _orderService.GetOrderAsync(id);
        }

        [HttpGet]
        public async Task<PagedResultsDto<OrderListItemDto>> Search([FromQuery] OrderSearchDto filter)
        {
            return await _orderService.SearchOrdersAsync(filter);
        }

        [HttpGet("{orderId}/StockRelease")]
        public async Task<ActionResult> DownloadStockRelease(Guid orderId)
        {
            var bytes = await _orderService.GenerateStockReleaseAsync(orderId);

            return File(bytes, ReportContentTypes.XLSX, "StockRelease.xlsx");
        }

        [HttpGet("NextOrderNo")]
        public async Task<int> GetNextOrderNo([FromQuery] DateTime date)
        {
            var orderNo = await _orderService.GetNextOrderNoAsync(date);
            return orderNo;
        }

        [HttpGet("{orderId}/PDDocuments")]
        [AllowAnonymous]
        public async Task<ActionResult> DownloadPDDocuments(Guid orderId)
        {
            var bytes = await _orderService.GeneratePDDocumentAsync(orderId);

            return File(bytes, ReportContentTypes.PDF, "PDDocument.pdf");
        }
    }
}
