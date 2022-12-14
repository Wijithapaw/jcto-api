using JCTO.Domain;
using JCTO.Domain.Entities;
using JCTO.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCTO.Tests.Helpers
{
    internal static class EntityHelper
    {
        public static User CreateUser(string firstName, string lastName, string email)
        {
            return new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email
            };
        }

        public static Customer CreateCustomer(string name, bool inactive = false)
        {
            return new Customer
            {
                Name = name,
                Inactive = inactive
            };
        }

        public static Product CreateProduct(string code, bool inactive = false)
        {
            return new Product
            {
                Code = code,
                Inactive = inactive
            };
        }

        public static Entry CreateEntry(string entryNo, Guid customerId, Guid productId, decimal initialQuantity, decimal remainingQuantity, DateTime entryDate, EntryStatus entryStatus)
        {
            return new Entry
            {
                EntryNo = entryNo,
                CustomerId = customerId,
                ProductId = productId,
                InitialQualtity = initialQuantity,
                RemainingQuantity = remainingQuantity,
                EntryDate = entryDate,
                Status = entryStatus,
            };
        }

        public static EntryTransaction CreateEntryTransaction(EntryTransactionType type, DateTime txnDate, string obRef,
            decimal quantity, decimal? deliveredQuantity, ApprovalType approvalType, string approvalRef = null, EntryTransaction approvalTxn = null, Guid? orderId = null)
        {
            return new EntryTransaction
            {
                ObRef = obRef,
                Quantity = quantity,
                DeliveredQuantity = deliveredQuantity,
                Type = type,
                TransactionDate = txnDate,
                OrderId = orderId,
                ApprovalType = approvalType,
                ApprovalRef = approvalRef,
                ApprovalTransaction = approvalTxn
            };
        }

        public static Order CreateOrder(Guid customerId, Guid productId, DateTime orderDate, int orderNo,
            string buyer, BuyerType buyerType, string tankNo, string obPrefix, OrderStatus status, decimal quantity, decimal? deliveredQty,
            string remarks, List<BowserEntry> bowserEntries)
        {
            return new Order
            {
                CustomerId = customerId,
                ProductId = productId,
                OrderDate = orderDate,
                OrderNo = orderNo,
                Buyer = buyer,
                ObRefPrefix = obPrefix,
                BuyerType = buyerType,
                TankNo = tankNo,
                Status = status,
                Remarks = remarks,
                Quantity = quantity,
                BowserEntries = bowserEntries,
            };
        }

        public static BowserEntry CreateBowserEntry(int capacity, int count)
        {
            return new BowserEntry
            {
                Capacity = capacity,
                Count = count,
            };
        }

        public static async Task<Guid> GetCustomerIdAsync(IDataContext dataContext, string name)
        {
            return (await dataContext.Customers.FirstAsync(c => c.Name == name)).Id;
        }

        public static async Task<Guid> GetEntryIdAsync(IDataContext dataContext, string entryNo)
        {
            return (await dataContext.Entries.FirstAsync(c => c.EntryNo == entryNo)).Id;
        }

        public static async Task<Guid> GetOrderIdAsync(IDataContext dataContext, int orderNo)
        {
            return (await dataContext.Orders.FirstAsync(o => o.OrderNo == orderNo)).Id;
        }

        public static async Task<Guid> GetProductIdAsync(IDataContext dataContext, string code)
        {
            return (await dataContext.Products.FirstAsync(c => c.Code == code)).Id;
        }

        public static async Task<Guid> GetGetEntryTxnIdAsync(IDataContext dataContext, string entryNo, ApprovalType approvalType, string approvalRef)
        {
            var entryTxn = await dataContext.EntryTransactions
                .Where(t => t.Entry.EntryNo == entryNo
                    && t.ApprovalType == approvalType
                    && t.ApprovalRef == approvalRef)
                .FirstAsync();
            return entryTxn.Id;
        }
    }
}
