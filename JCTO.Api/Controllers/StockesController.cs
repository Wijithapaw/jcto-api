using JCTO.Domain.Dtos;
using JCTO.Domain.Dtos.Base;
using JCTO.Domain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JCTO.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockesController : ControllerBase
    {
        private readonly IStockService _stockService;

        public StockesController(IStockService stockService)
        {
            _stockService = stockService;
        }

        [HttpPost("Topup")]
        public async Task<EntityCreateResult> Topup(StockTopupDto dto)
        {
            return await _stockService.TopupAsync(dto);
        }

        [HttpGet("Discharges")]
        public async Task<PagedResultsDto<StockDischargeListItemDto>> SearchDischarges([FromQuery] StockDischargeSearchDto filter)
        {
            return await _stockService.SearchStockDischargesAsync(filter);
        }
    }
}
