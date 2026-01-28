using System;
using System.Collections.Generic;

namespace LbI.DailyCashes.Dto
{
    public class VouchersOutputDto
    {
        public bool IsAny { get; set; }
        public bool MostRecennt { get; set; }
        public List<VoucherOutputDto> Vouchers { get; set; }
    }

    public class VoucherOutputDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string VoucherNumber { get; set; }
        public string Creator { get; set; }
        public string CarNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public string IncomeRecords { get; set; }
        public string ExpenseRecords { get; set; }
    }

    public class VoucherFirstLastDateDto
    {
        public DateTime? FirstDate { get; set; }
        public DateTime? CurrentDate { get; set; }
    }
}
