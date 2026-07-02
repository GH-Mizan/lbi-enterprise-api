using LbI.Enums;
using System;

namespace LbI.Sales.Dto
{
    public class DueReceivedEntryDto
    {
        public int SalesId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime ReceiveDate { get; set; }
        public string InvoiceNumber { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string PaymentStatusText { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string SalesBy { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal PrevDiscount { get; set; }
        public decimal Discount { get; set; }
        public decimal NetTotal { get; set; }
        public decimal PrevTotalPaid { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal Due { get; set; }
        public string Remarks { get; set; }
        public DueReceivedHistoryDto DueReceived { get; set; }
    }
}
