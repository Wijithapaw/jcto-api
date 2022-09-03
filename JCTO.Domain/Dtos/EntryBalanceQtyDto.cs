namespace JCTO.Domain.Dtos
{
    public class EntryBalanceQtyDto
    {
        public Guid Id { get; set; }
        public string EntryNo { get; set; }
        public double RemainingQty { get; set; }
        public double InitialQty { get; set; }
        public double Rebond { get; set; }
        public double Xbond { get; set; }
        public double Letter { get; set; }
    }
}
