namespace JCTO.Domain.Dtos
{
    public class EntryBalanceQtyDto
    {
        public Guid Id { get; set; }
        public string EntryNo { get; set; }
        public decimal RemainingQty { get; set; }
        public decimal InitialQty { get; set; }
        public decimal Rebond { get; set; }
        public decimal Xbond { get; set; }
        public decimal Letter { get; set; }
    }
}
