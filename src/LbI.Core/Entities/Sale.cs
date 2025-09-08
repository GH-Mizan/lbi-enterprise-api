using Abp.Domain.Entities.Auditing;
using LbI.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace LbI.Entities
{
    public class Sale : FullAuditedEntity
    {
        public DateTime Date { get; set; }
        public string InvoiceNumber { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalAmount { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Discount { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal NetAmount { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal PaidAmount { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal DueAmount { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public int StockPointId { get; set; }
        public string SalesBy { get; set; }
        public string Remarks { get; set; }
        public bool Locked { get; set; }

    }
}
