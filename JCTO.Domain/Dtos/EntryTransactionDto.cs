namespace JCTO.Domain.Dtos
{
    public class EntryTransactionDto
    {
        public DateTime OrderDate { get; set; }
        public string OrderNo { get; set; }
        public string ObRef { get; set; }
        public double Quantity { get; set; }
        public double DeliveredQuantity { get; set; }
    }
}
