using System;
using System.Collections.Generic;

namespace LbI.Sales.Dto
{
    public class MonthlySalesInvoiceReportDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public DateTime PrepareDate { get; set; }
        public int MedicalOxygen9_8TotalQty { get; set; }
        public int MedicalOxygen1_36TotalQty { get; set; }
        public int MedicalAir9_8TotalQty { get; set; }
        public int MedicalAir7TotalQty { get; set; }
        public int Nitros30KgTotalQty { get; set; }
        public int Nitros5KgTotalQty { get; set; }
        public int Nitros3KgTotalQty { get; set; }
        public decimal TotalAmount { get; set; }

        public List<MonthlySalesInvoiceDetailsReportDto> Details { get; set; }
    }

    public class MonthlySalesInvoiceDetailsReportDto
    {
        public int? Id { get; set; }
        public DateTime Date { get; set; }
        public int MedicalOxygen9_8Qty { get; set; }
        public int MedicalOxygen1_36Qty { get; set; }
        public int MedicalAir9_8Qty { get; set; }
        public int MedicalAir7Qty { get; set; }
        public int Nitros30KgQty { get; set; }
        public int Nitros5KgQty { get; set; }
        public int Nitros3KgQty { get; set; }
        public decimal Amount { get; set; }
    }
}
