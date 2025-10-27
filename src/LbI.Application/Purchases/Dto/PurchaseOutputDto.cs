using LbI.Enums;
using System;

namespace LbI.Purchases.Dto
{
    public class PurchaseOutputDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string InvoiceNumber { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierShortName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string PaymentStatusText { get; set; }
        public int StockPointId { get; set; }
        public string StockPointName { get; set; }
        public string PurchaseBy { get; set; }
        public string Remarks { get; set; }
        public string PaymentHistory { get; set; }
        public bool Locked { get; set; }
    }
}
