namespace JCTO.Reports.Dtos
{
    public class OrderReportDto
    {
        public ReportFilter Filter { get; set; }
        public List<OrderDetails> Orders { get; set; }
    }

    public class ReportFilter
    {
        public string Customer { get; set; }
        public string Product { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalUndeliveredQuantity { get; set; }
        public decimal TotalCancelledQuantity { get; set; }
        public string DateRange { get; set; }
        public string BuyerType { get; set; }
        public string BuyerName { get; set; }
        public string Status { get; set; }
    }

    public class OrderDetails
    {
        public string Customer { get; set; }
        public string Product { get; set; }
        public decimal Quantity { get; set; }
        public string OrderDate { get; set; }
        public int OrderNo { get; set; }
        public string BuyerType { get; set; }
        public string BuyerName { get; set; }
        public string Status { get; set; }
        public string IssueCommencedTime { get; set; }
        public string IssueCompletedTime { get; set; }
    }
}

