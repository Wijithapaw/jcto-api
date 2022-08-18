using JCTO.Domain;
using JCTO.Domain.Dtos;
using JCTO.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace JCTO.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IDataContext _dataContext;

        public CustomerService(IDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<List<ListItem>> GetAllCustomersListItemsAsync()
        {
            var listItems = await _dataContext.Customers
               .Where(c => c.Active)
               .Select(c => new ListItem { Id = c.Id.ToString(), Label = c.Name })
               .ToListAsync();

            return listItems;            
        }

        public async Task<List<CustomerStockDto>> GetAllCustomerStocksAsync()
        {
            var customerStocks = await _dataContext.Customers
                .Where(c => c.Active)
                .Select(c => new CustomerStockDto
                {
                    CustomerId = c.Id,
                    CustomerName = c.Name,
                    Stocks = new List<ProductStockDto>()
                }).ToListAsync();

            return customerStocks;
        }
    }
}
