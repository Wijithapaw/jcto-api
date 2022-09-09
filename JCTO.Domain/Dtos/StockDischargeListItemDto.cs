namespace JCTO.Domain.Dtos
{
    public class StockDischargeListItemDto
    {
        public Guid Id { get; set; }
        public string Customer { get; set; }
        public string Product { get; set; }
        public string ToBondNo { get; set; }
        public double Quantity { get; set; }
        public DateTime Date { get; set; }
    }
}
