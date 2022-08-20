﻿using JCTO.Domain.Entities;
using JCTO.Domain.Enums;
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

        public static Entry CreateEntry(string entryNo, Guid customerId, Guid productId, double initialQuantity, DateTime entryDate, EntryStatus entryStatus)
        {
            return new Entry
            {
                EntryNo = entryNo,
                CustomerId = customerId,
                ProductId = productId,
                InitialQualtity = initialQuantity,
                EntryDate = entryDate,
                Status = entryStatus
            };
        }

        public static EntryTransaction CreateEntryTransaction(EntryTransactionType type, double amount)
        {
            return new EntryTransaction
            {
                Amount = amount,
                Type = type,
                TransactionDateTimeUtc = DateTime.UtcNow,
            };
        }
    }
}