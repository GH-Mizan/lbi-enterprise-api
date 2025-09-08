using Abp.Domain.Entities.Auditing;
using LbI.Enums;
using System;

namespace LbI.Entities
{
    public class DueReceivedHistory : FullAuditedEntity
    {
        public int SalesId { get; set; }
        //public DateTime CreationTime { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime ReceiveDate { get; set; }
        public string InvoiceNumber { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal NetTotal { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal Due { get; set; }
        public string Remarks { get; set; }
    }
}
