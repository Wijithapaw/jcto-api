namespace JCTO.Domain.Dtos
{
    public class EntryRebondToDto
    {
        public Guid CustomerId { get; set; }
        public Guid EntryId { get; set; }
        public double Quantity { get; set; }
        public DateTime Date { get; set; }
        public string RebondNo { get; set; }
    }
}
