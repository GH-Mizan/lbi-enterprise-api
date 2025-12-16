using System;
using System.Collections.Generic;

namespace LbI.Sales.Dto
{
    public class SalesReceiptOutputDto
    {
        public int Id { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string InvoiceNumber { get; set; }
        public string ReferenceNumber { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string Saler { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalDue { get; set; }
        public decimal PreviousDue { get; set; }
        public decimal OverallDue { get; set; }

        public List<SalesRecieptProductDto> Details { get; set; }
        public List<DueReceivedBreakdownDto> ReceivedBreakdown { get; set; }
    }

    public class SalesRecieptProductDto
    {
        public int ProductId { get; set; }
        public string Product { get; set; }
        public decimal UnitPrice { get; set; }
        public int Qty { get; set; }
        public decimal Amount { get; set; }
    }

    public class DueReceivedBreakdownDto
    {
        public DateTime ReceivedDate { get; set; }
        public decimal Amount { get; set; }
    }


}
