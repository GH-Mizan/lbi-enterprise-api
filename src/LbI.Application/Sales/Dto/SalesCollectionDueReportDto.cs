using System;
using System.Collections.Generic;

namespace LbI.Sales.Dto
{
    public class SalesCollectionDueReportDto
    {
        public List<SalesCollectionDueDetailsDto> Details { get; set; }
        public DateTime LastDate { get; set; }
        public decimal DueBalance { get; set; }
        public decimal PrevBalance { get; set; }

    }

    public class SalesCollectionDueDetailsDto
    {
        public int Id { get; set; }
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
