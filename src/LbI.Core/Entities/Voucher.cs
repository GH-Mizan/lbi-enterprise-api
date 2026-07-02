using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace LbI.Entities
{
    public class Voucher : FullAuditedEntity
    {
        public DateTime Date { get; set; }
        public int DailyCashId { get; set; }
        public string VoucherNumber { get; set; }
        public int CreatorId { get; set; }
        public string CarNumber { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal DayEndCash { get; set; }
        public string IncomeRecords { get; set; } //JsonValue
        public string ExpenseRecords { get; set; } //JsonValue
    }
}
