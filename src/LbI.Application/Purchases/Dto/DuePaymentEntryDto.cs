using LbI.Enums;
using System;

namespace LbI.Purchases.Dto
{
    public class DuePaymentEntryDto
    {
        public int? Id { get; set; }
        public int PurchaseId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime PaymentDate { get; set; }
        public string InvoiceNumber { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string PaymentStatusText { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string PurchasedBy { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal PrevDiscount { get; set; }
        public decimal Discount { get; set; }
        public decimal NetTotal { get; set; }
        public decimal PrevTotalPaid { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal Due { get; set; }
        public string Remarks { get; set; }
        public DuePaymentHistoryDto DuePayment { get; set; }
    }
}
