using LbI.Enums;
using System;

namespace LbI.Purchases.Dto
{
    public class DuePaymentHistoryDto
    {
        public int? Id { get; set; }
        public int PurchaseId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime PaymentDate { get; set; }
        public string InvoiceNumber { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal NetTotal { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal Due { get; set; }
        public bool Default { get; set; }
        public string Remarks { get; set; }
        public string UserName { get; set; }
    }
}
