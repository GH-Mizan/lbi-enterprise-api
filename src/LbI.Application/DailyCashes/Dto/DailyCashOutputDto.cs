using System;
using System.Collections.Generic;

namespace LbI.DailyCashes.Dto
{
    public class DailyCashOutputDto
    {
        public int? Id { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal TotalAdvance { get; set; }
        public decimal TotalDayEndCash { get; set; }
        public decimal PaperBalance { get; set; }
        public decimal ActualBalance { get; set; }
        public decimal Difference { get; set; }
        public decimal TotalDue { get; set; }
        public decimal BalanceCD { get; set; }
        public decimal ActualDifference { get; set; }
        public bool Completed { get; set; }
        public string Remarks { get; set; }
        public bool IsLast { get; set; }
        public bool IsNew { get; set; }

        public List<DailyCashIncomeDetailDto> Incomes { get; set; }
        public List<DailyCashExpenseDetailDto> Expenses { get; set; }
        public List<DailyCashAdvanceDetailDto> Advances { get; set; }
        public List<DailyCashDayEndDetailDto> DayEndCashes { get; set; }
        public List<VoucherOutputDto> Vouchers { get; set; }
    }
}
