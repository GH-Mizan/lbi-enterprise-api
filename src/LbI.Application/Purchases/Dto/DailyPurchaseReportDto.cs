using LbI.Enums;
using System.Collections.Generic;

namespace LbI.Purchases.Dto
{
    public class DailyPurchaseReportDto
    {
        public List<DailyPurchaseReportDetailsDto> Details { get; set; }
        public int MedicalOxygen9_8TotalQty { get; set; }
        public int MedicalOxygen1_36TotalQty { get; set; }
        public int MedicalAir9_8TotalQty { get; set; }
        public int MedicalAir7TotalQty { get; set; }
        public int Nitros30KgTotalQty { get; set; }
        public int Nitros5KgTotalQty { get; set; }
        public int Nitros3KgTotalQty { get; set; }
        public decimal NetTotal { get; set; }
        public decimal CashPayment { get; set; }
        public decimal DuePayment { get; set; }
        public decimal Due { get; set; }
    }

    public class DailyPurchaseReportDetailsDto
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public ProductType Type { get; set; }
        public string TypeText { get; set; }
        public ProductSize Size { get; set; }
        public string SizeText { get; set; }
        public int MedicalOxygen9_8Qty { get; set; }
        public int MedicalOxygen1_36Qty { get; set; }
        public int MedicalAir9_8Qty { get; set; }
        public int MedicalAir7Qty { get; set; }
        public int Nitros30KgQty { get; set; }
        public int Nitros5KgQty { get; set; }
        public int Nitros3KgQty { get; set; }
        public string InvoiceNo { get; set; }
        public decimal NetAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
        public decimal DuePayment { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string PaymentStatusText { get; set; }
    }
}
