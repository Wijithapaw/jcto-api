﻿using JCTO.Domain.Constants;
using JCTO.Domain.Dtos;
using JCTO.Domain.Dtos.Base;
using JCTO.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

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
            await Task.Delay(100);
            return new EntityUpdateResult { ConcurrencyKey = Guid.NewGuid() };
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
        [AllowAnonymous]
        public async Task<ActionResult> DownloadStockRelease(Guid orderId)
        {
            var bytes = await _orderService.GenerateStockReleaseAsync(orderId);

            return File(bytes, ReportContentTypes.XLSX, "StockRelease.xlsx");
        }
    }
}
