namespace JCTO.Domain.Enums
{
    public enum EntryStatus
    {
        Active = 0,
        Completed = 1
    }

    public enum EntryTransactionType
    {
        Approval = 0,
        Out = 1,
        RebondTo = 2,
        Reversal = 3,
    }

    public enum OrderStatus
    {
        Undelivered = 0,
        Delivered = 1,
        Cancelled = 2,
    }

    public enum BuyerType
    {
        Barge = 0,
        Bowser = 1,
    }

    public enum ApprovalType
    {
        Rebond = 1,
        XBond = 2,
        Letter = 3
    }
}
