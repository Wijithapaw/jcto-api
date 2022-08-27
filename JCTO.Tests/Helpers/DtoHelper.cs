﻿using JCTO.Domain.Dtos;
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

        internal static EntryDto CreateEntryDto(string entryNo, Guid customerId, Guid productId, DateTime entryDate, EntryStatus entryStatus, double initialQuantity)
        {
            return new EntryDto
            {
                EntryNo = entryNo,
                CustomerId = customerId,
                ProductId = productId,
                EntryDate = entryDate,
                Status = entryStatus,
                InitialQuantity = initialQuantity
            };
        }

        internal static OrderDto CreateOrderDto(Guid id, Guid customerId, Guid productId, string orderNo, DateTime orderDate, double quantity,
            string buyer, OrderStatus status, string obPrefix, string tankNo, BuyerType buyerType, string xBondNo, string? remarks,
            List<OrderStockReleaseEntryDto> releaseEntries, List<BowserEntryDto> bowserEntries)
        {
            return new OrderDto
            {
                Id = id,
                CustomerId = customerId,
                ProductId = productId,
                OrderNo = orderNo,
                OrderDate = orderDate,
                Quantity = quantity,
                Buyer = buyer,
                BuyerType = buyerType,
                Status = status,
                ObRefPrefix = obPrefix,
                TankNo = tankNo,
                XBondNo = xBondNo,
                Remarks = remarks,
                ReleaseEntries = releaseEntries,
                BowserEntries = bowserEntries
            };
        }
    }
}
