using System;

namespace LbI.DailyCashes.Dto
{
    public class CreateOrUpdateDailyCashInput
    {
        public DailyCashEntryDto DailyCashInfo { get; set; }
        public DailyCashEntryDto PrevCashInfo { get; set; }
    }

    public class DailyCashEntryDto
    {
        public int? Id { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalActualIncome { get; set; }
        public decimal TotalVirtualTransaction { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal DayStartCashBalance { get; set; }
        public decimal DayStartAdvanceBalance { get; set; }
        public decimal DayEndCashBalance { get; set; }
        public decimal DayEndAdvanceBalance { get; set; }
        public decimal Difference { get; set; }
        public string Metadata { get; set; }
    }
}
