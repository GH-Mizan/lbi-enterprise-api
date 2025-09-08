using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace LbI.Entities
{
    public class DailyCash : FullAuditedEntity
    {
        public DateTime Date { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public  decimal TotalActualIncome{ get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalVirtualTransaction { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalExpense { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal DayStartCashBalance { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal DayStartAdvanceBalance { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal DayEndCashBalance { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal DayEndAdvanceBalance { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Difference { get; set; }
        public string Metadata { get; set; }
    }
}
