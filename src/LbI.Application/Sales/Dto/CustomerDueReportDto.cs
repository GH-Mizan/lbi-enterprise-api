using System;
using System.Collections.Generic;

namespace LbI.Sales.Dto
{
    public class CustomerDueReportDto
    {
        public int MedicalOxygen9_8TotalQty { get; set; }
        public int MedicalOxygen1_36TotalQty { get; set; }
        public int MedicalAir9_8TotalQty { get; set; }
        public int MedicalAir7TotalQty { get; set; }
        public int Nitros30KgTotalQty { get; set; }
        public int Nitros5KgTotalQty { get; set; }
        public int Nitros3KgTotalQty { get; set; }
        public decimal OverallDue { get; set; }
        public decimal OverallBalance { get; set; }
        public decimal ActualDue { get; set; }
        public List<CustomerDueDetailsDto> Details { get; set; }
    }

    public class CustomerDueDetailsDto
    {
        public DateTime Date { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int MedicalOxygen9_8Qty { get; set; }
        public int MedicalOxygen1_36Qty { get; set; }
        public int MedicalAir9_8Qty { get; set; }
        public int MedicalAir7Qty { get; set; }
        public int Nitros30KgQty { get; set; }
        public int Nitros5KgQty { get; set; }
        public int Nitros3KgQty { get; set; }
        public string InvoiceNo { get; set; }
        public decimal TotalDue { get; set; }
        public decimal Balance { get; set; }
    }
}
