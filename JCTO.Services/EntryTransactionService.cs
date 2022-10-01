using JCTO.Domain.Entities;
using JCTO.Domain.Enums;

namespace JCTO.Services
{
    public static class EntryTransactionService
    {
        public static EntryTransaction GetEntryTransaction(EntryTransactionType type, Guid id, Entry entry, Order order, DateTime transactionDate,
            ApprovalType? approvalType, string approvalRef, Guid? approvalId, string obRef, double quantity, double? deliveredQuantity)
        {
            var txn = new EntryTransaction
            {
                Id = id,
                Entry = entry,
                Order = order,
                Quantity = SignAmount(type, quantity),
                DeliveredQuantity = order?.Status == OrderStatus.Delivered ? SignAmount(type, deliveredQuantity.Value) : null,
                ObRef = obRef,
                Type = type,
                ApprovalType = approvalType,
                ApprovalRef = approvalRef,
                TransactionDate = transactionDate,
                ApprovalTransactionId = approvalId,
            };

            return txn;
        }

        private static double SignAmount(EntryTransactionType type, double amount)
        {
            var amountAbs = Math.Abs(amount);
            var signedAmount = amountAbs * GetSign(type);
            return signedAmount;
        }

        private static int GetSign(EntryTransactionType type)
        {
            switch (type)
            {
                case EntryTransactionType.Approval:
                case EntryTransactionType.Reversal: return 1;
                case EntryTransactionType.Out:
                case EntryTransactionType.RebondTo: return -1;
                default: throw new ApplicationException("Sign has not ben defined for EntryTransactionType");
            }
        }
    }
}
