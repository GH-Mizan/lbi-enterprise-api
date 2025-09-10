namespace LbI.Sales.Dto
{
    public class CustomerOverallDueReportDto
    {
        public string Serial { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public decimal PreviousDue { get; set; }
        public decimal CurrentSales { get; set; }
        public decimal CurrentPaymnet { get; set; }
        public decimal CurrentDue { get; set; }
    }
}
