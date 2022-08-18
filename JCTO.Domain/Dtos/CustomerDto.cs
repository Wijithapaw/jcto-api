using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCTO.Domain.Dtos
{
    public class CustomerStockDto
    {
        public Guid? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public List<ProductStockDto>? Stocks { get; set; }

    }

    public class ProductStockDto
    {
        public string? ProductCode { get; set; }
        public double RemainingStock { get; set; }
        public double UndeliveredStock { get; set; }
    }
}
