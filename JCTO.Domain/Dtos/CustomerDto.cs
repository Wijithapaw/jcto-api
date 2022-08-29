namespace JCTO.Domain.Dtos
{
    public class CustomerStockDto
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public List<ProductStockDto> Stocks { get; set; }
    }

    public class ProductStockDto
    {
        public Guid ProductId { get; set; }
        public double RemainingStock { get; set; }
        public double UndeliveredStock { get; set; }
    }
}
