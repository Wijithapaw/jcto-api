namespace JCTO.Domain.Dtos
{
    public class StockReleaseReportDto
    {
        public string Customer { get; set; }
        public string Product { get; set; }
        public double Quantity { get; set; }
        public string OrderDate { get; set; }
        public string OrderNo { get; set; }
        public string TankNo { get; set; }
        public string Buyer { get; set; }
        public string EntryNo { get; set; }
        public string ObRef { get; set; }
        public string QuantityInText { get; set; }
        public string Remarks { get; set; }
        public bool TaxPaid { get; set; }
    }
}
