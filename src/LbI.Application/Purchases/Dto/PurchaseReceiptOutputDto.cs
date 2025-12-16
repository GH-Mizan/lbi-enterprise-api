using System;
using System.Collections.Generic;

namespace LbI.Sales.Dto
{
    public class PurchaseReceiptOutputDto
    {
        public int Id { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string InvoiceNumber { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string Address { get; set; }
        public string Purchaser { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalDue { get; set; }
        public decimal PreviousDue { get; set; }
        public decimal OverallDue { get; set; }

        public List<PurchaseRecieptProductDto> Details { get; set; }
        public List<DuePaymentBreakdownDto> PaymentBreakdown { get; set; }
    }

    public class PurchaseRecieptProductDto
    {
        public int ProductId { get; set; }
        public string Product { get; set; }
        public decimal UnitPrice { get; set; }
        public int Qty { get; set; }
        public decimal Amount { get; set; }
    }

    public class DuePaymentBreakdownDto
    {
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
    }
}
