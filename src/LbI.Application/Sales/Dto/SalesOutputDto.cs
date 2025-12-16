using LbI.Enums;
using System;

namespace LbI.Sales.Dto
{
    public class SalesOutputDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string InvoiceNumber { get; set; }
        public string ReferenceNumber { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerShortName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string PaymentStatusText { get; set; }
        public int StockPointId { get; set; }
        public string StockPointName { get; set; }
        public string SalesBy { get; set; }
        public string Remarks { get; set; }
        public string PaymentReceiveHistory { get; set; }
        public bool Locked { get; set; }
        public string UserName { get; set; }
    }
}
