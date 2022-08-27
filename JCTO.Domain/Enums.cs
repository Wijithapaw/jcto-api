using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCTO.Domain.Enums
{
    public enum EntryStatus
    {
        Active = 0,
        Completed = 1
    }
    public enum EntryTransactionType
    {
        In = 0,
        Out = 1,
    }

    public enum OrderStatus
    {
        Undelivered = 0,
        Delivered = 1,
    }

    public enum BuyerType
    {
        Barge = 0,
        Bowser = 1,
    }
}
