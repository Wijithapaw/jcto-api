using JCTO.Domain.Dtos;
using JCTO.Domain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JCTO.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet("Stocks")]
        public async Task<List<CustomerStockDto>> GetAllCustomerStocks()
        {
            return await _customerService.GetAllCustomerStocksAsync();
        }

        [HttpGet("ListItems")]
        public async Task<List<ListItem>> GetAllCustomersListItems()
        {
            return await _customerService.GetAllCustomersListItemsAsync();
        }

        [HttpGet("Products/ListItems")]
        public async Task<List<ListItem>> GetAllProductsListItems()
        {

            return await _customerService.GetProductListItemsAsync();
        }
    }
}
