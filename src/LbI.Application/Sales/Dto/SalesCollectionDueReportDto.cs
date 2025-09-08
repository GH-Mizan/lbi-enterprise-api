using System;

namespace LbI.Sales.Dto
{
    public class SalesCollectionDueReportDto
    {
        public DateTime Date { get; set; }
        public decimal TotalSales { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal CashCollection { get; set; }
        public decimal DueCollection { get; set; }
        public decimal TotalCollection { get; set; }
        public decimal CollectedBalance { get; set; }
        public decimal CurrenctDue { get; set; }
        public decimal DetuctedDue { get; set; }
        public decimal DueBalance { get; set; }

    }
}
