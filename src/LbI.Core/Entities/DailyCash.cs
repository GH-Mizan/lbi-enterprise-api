using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace LbI.Entities
{
    public class DailyCash : FullAuditedEntity
    {
        public DateTime Date { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public  decimal TotalIncome{ get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalExpense { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalAdvance { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalDayEndCash { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal PaperBalance { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal ActualBalance { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Difference { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalDue { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal BalanceCD { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal ActualDifference { get; set; }
        public bool Completed { get; set; }
        public string Remarks { get; set; }
    }
}
