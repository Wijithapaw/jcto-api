namespace JCTO.Domain.Dtos
{
    public class StockTopupDto
    {
        public Guid CustomerId { get; set; }
        public Guid ProductId { get; set; }
        public double Quantity { get; set; }
        public string ToBondNo { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
