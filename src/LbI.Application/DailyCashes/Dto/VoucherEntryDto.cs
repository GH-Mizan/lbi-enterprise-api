using System;

namespace LbI.DailyCashes.Dto
{
    public class VoucherEntryDto
    {
        public DateTime Date { get; set; }
        public string VoucherNumber { get; set; }
        public string Creator { get; set; }
        public string CarNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public string IncomeRecords { get; set; } //JsonValue
        public string ExpenseRecords { get; set; } //JsonValue
    }
}
