using JCTO.Domain.Dtos;
using JCTO.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCTO.Tests.Helpers
{
    internal static class DtoHelper
    {
        internal static UserDto CreateUserDto(string firstName, string lastName, string email, Guid? concurrencyKey = null)
        {
            return new UserDto
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                ConcurrencyKey = concurrencyKey ?? Guid.NewGuid(),
            };
        }

        internal static EntryDto CreateEntryDto(string toBondNo, string entryNo, DateTime entryDate, EntryStatus entryStatus, double initialQuantity)
        {
            return new EntryDto
            {
                ToBondNo = toBondNo,
                EntryNo = entryNo,
                EntryDate = entryDate,
                Status = entryStatus,
                InitialQuantity = initialQuantity
            };
        }

        internal static OrderDto CreateOrderDto(Guid id, Guid customerId, Guid productId, int orderNo, DateTime orderDate,
            double quantity, double? deliveredQuantity, string buyer, OrderStatus status, string obPrefix, string tankNo, BuyerType buyerType,
            string remarks, List<OrderStockReleaseEntryDto> releaseEntries, List<BowserEntryDto> bowserEntries, Guid concurrencyKey)
        {
            return new OrderDto
            {
                Id = id,
                CustomerId = customerId,
                ProductId = productId,
                OrderNo = orderNo,
                OrderDate = orderDate,
                Quantity = quantity,
                DeliveredQuantity = deliveredQuantity,
                Buyer = buyer,
                BuyerType = buyerType,
                Status = status,
                ObRefPrefix = obPrefix,
                TankNo = tankNo,
                Remarks = remarks,
                ReleaseEntries = releaseEntries,
                BowserEntries = bowserEntries,
                ConcurrencyKey = concurrencyKey
            };
        }

        internal static EntryApprovalDto CreateEntryApprovalDto(Guid entryId, ApprovalType approvalType,
            string approvalRef, DateTime date, double qty)
        {
            return new EntryApprovalDto
            {
                EntryId = entryId,
                ApprovalRef = approvalRef,
                Type = approvalType,
                ApprovalDate = date,
                Quantity = qty
            };
        }
    }
}
