using JCTO.Domain.Entities;
using JCTO.Domain.Enums;

namespace JCTO.Services
{
    public static class EntryTransactionService
    {
        public static List<EntryTransaction> GetEntryTransactions(EntryTransactionType type, double amount)
        {
            var txn = new EntryTransaction
            {
                Amount = SignAmount(type, amount),
                Type = type,
                TransactionDateTimeUtc = DateTime.UtcNow,                
            };

            var txns = new List<EntryTransaction> { txn };
            return txns;
        }

        private static double SignAmount(EntryTransactionType type, double amount)
        {
            var amountAbs = Math.Abs(amount);
            var signedAmount = (type == EntryTransactionType.Out ? -1 : 1) * amountAbs;
            return signedAmount;
        }
    }
}
