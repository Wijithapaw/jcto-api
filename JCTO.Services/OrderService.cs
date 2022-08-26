using JCTO.Domain;
using JCTO.Domain.CustomExceptions;
using JCTO.Domain.Dtos;
using JCTO.Domain.Enums;
using JCTO.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCTO.Services
{
    public class OrderService : IOrderService
    {
        private readonly IDataContext _dataContext;

        public OrderService(IDataContext dataContext)
        {
            _dataContext = dataContext;
        }
        public Task<EntityCreateResult> CreateAsync(OrderDto dto)
        {
            ValidateOrder(dto);

            throw new NotImplementedException();
        }

        private void ValidateOrder(OrderDto order)
        {
            var errors = new List<string>();

            if (order.CustomerId == Guid.Empty)
                errors.Add("Customer not found");

            if (order.ProductId == Guid.Empty)
                errors.Add("Product not found");

            if (order.OrderDate == DateTime.MinValue)
                errors.Add("Order Date not found");

            if (string.IsNullOrWhiteSpace(order.OrderNo))
                errors.Add("Order No. not found");

            if (string.IsNullOrWhiteSpace(order.Buyer))
                errors.Add("Buyer not found");

            if (order.Quantity <= 0)
                errors.Add("Quantity must be > 0");

            if(string.IsNullOrWhiteSpace(order.ObRefPrefix))
                errors.Add("OBRef not found");

            if (string.IsNullOrWhiteSpace(order.TankNo))
                errors.Add("Tank No. not found");

            if(!order.ReleaseEntries.Any())
                errors.Add("Stock release entries not found");

            if(order.ReleaseEntries.Sum(e => e.DeliveredQuantity) != order.Quantity)
                errors.Add("Sum of Delivered Quantities not equal to overall Quantity");

            if (order.ReleaseEntries.Any(e => e.DeliveredQuantity > e.Quantity))
                errors.Add("Stock release entries found having Delevered Quantity > Quantity");

            if(order.BuyerType == BuyerType.Local)
            {
                //TODO: Validate xBondNo, Bowser entries
            }

            if(errors.Any())
            {
                throw new JCTOValidationException(string.Join(", ", errors));
            }
        }
    }
}
