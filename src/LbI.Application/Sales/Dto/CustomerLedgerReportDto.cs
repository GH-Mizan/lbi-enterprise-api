

using System;
using System.Collections.Generic;

namespace LbI.Sales.Dto
{
    public class CustomerLedgerReportDto
    {
        public int MedicalOxygen9_8TotalQty { get; set; }
        public int MedicalOxygen1_36TotalQty { get; set; }
        public int MedicalAir9_8TotalQty { get; set; }
        public int MedicalAir7TotalQty { get; set; }
        public int Nitros30KgTotalQty { get; set; }
        public int Nitros5KgTotalQty { get; set; }
        public int Nitros3KgTotalQty { get; set; }
        public decimal OverallCreditTotal { get; set; } //NetAmount
        public decimal OverallDebitTotal { get; set; } //PaidAmount
        public decimal OverallBalance { get; set; }
        public decimal ActualCreditTotal { get; set; }
        public decimal ActualDebitTotal { get; set; }
        public decimal InitialDue { get; set; }

        public List<CustomerLedgerDetailsDto> Details { get; set; }
    }

    public class CustomerLedgerDetailsDto
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
        public decimal CreditTotal { get; set; }
        public decimal DebitTotal { get; set; }
        public decimal Balance { get; set; }
    }
}
