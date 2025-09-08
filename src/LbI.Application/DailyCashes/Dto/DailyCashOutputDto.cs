using System;

namespace LbI.DailyCashes.Dto
{
    public class DailyCashOutputDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalActualIncome { get; set; }
        public decimal TotalVirtualTransaction { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal DayStartCashBalance { get; set; }
        public decimal DayStartAdvanceBalance { get; set; }
        public decimal DayEndCashBalance { get; set; }
        public decimal DayEndAdvanceBalance { get; set; }
        public decimal Difference { get; set; }
    }
}
