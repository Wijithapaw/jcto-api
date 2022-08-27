using JCTO.Domain.Entities;
using JCTO.Domain.Enums;

namespace JCTO.Services
{
    public static class EntryTransactionService
    {
        public static EntryTransaction GetEntryTransaction(EntryTransactionType type, Guid id, Guid entryId, string obRef, double quantity, double deliveredQuantity)
        {
            var txn = new EntryTransaction
            {
                Id = id,
                EntryId = entryId,
                Quantity = SignAmount(type, quantity),
                DeliveredQuantity = SignAmount(type, deliveredQuantity),
                ObRef = obRef,
                Type = type,
                TransactionDateTimeUtc = DateTime.UtcNow,
            };

            return txn;
        }

        private static double SignAmount(EntryTransactionType type, double amount)
        {
            var amountAbs = Math.Abs(amount);
            var signedAmount = (type == EntryTransactionType.Out ? -1 : 1) * amountAbs;
            return signedAmount;
        }
    }
}
