using LbI.Enums;
using System;
using System.Collections.Generic;

namespace LbI.Sales.Dto
{
    public class SalesEntryInput
    {
        public SalesEntryDto Sales { get; set; }
        public List<SalesDetailsEntryDto> SalesDetails { get; set; }
        public DueReceivedHistoryDto DueReceived { get; set; }
    }

    public class SalesEntryDto
    {
        public int? Id { get; set; }
        public DateTime Date { get; set; }
        public string InvoiceNumber { get; set; }
        public string ReferenceNumber { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public int SalesBy { get; set; }
        public int StockPointId { get; set; }
        public string Remarks { get; set; }
        public string PaymentReceiveHistory { get; set; }
        public bool Locked { get; set; }
    }

    public class SalesDetailsEntryDto
    {
        public int? Id { get; set; }
        public int SalesId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string Remarks { get; set; }
    }

}
