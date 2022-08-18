using JCTO.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCTO.Domain.Services
{
    public interface ICustomerService
    {
        Task<List<CustomerStockDto>> GetAllCustomerStocksAsync();
        Task<List<ListItem>> GetAllCustomersListItemsAsync();
    }
}
