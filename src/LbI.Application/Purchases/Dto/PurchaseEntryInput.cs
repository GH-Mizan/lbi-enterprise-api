using LbI.Enums;
using LbI.Sales.Dto;
using System;
using System.Collections.Generic;

namespace LbI.Purchases.Dto
{
    public class PurchaseEntryInput
    {
        public PurchaseEntryDto Purchase { get; set; }
        public List<PurchaseDetailsEntryDto> PurchaseDetails { get; set; }
        public DuePaymentHistoryDto DuePayment { get; set; }
    }

    public class PurchaseEntryDto
    {
        public int? Id { get; set; }
        public DateTime Date { get; set; }
        public string InvoiceNumber { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string PurchaseBy { get; set; }
        public int StockPointId { get; set; }
        public string Remarks { get; set; }
        public string PaymentHistory { get; set; }
        public bool Locked { get; set; }
    }

    public class PurchaseDetailsEntryDto
    {
        public int? Id { get; set; }
        public int PurchaseId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string Remarks { get; set; }
    }
}
