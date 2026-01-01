using System;

namespace LbI.Salaries.Dto
{
    public class SalaryEntryOutputDto
    {
        public int? Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public DateTime? SalaryDate { get; set; }
        public bool DateEditMode { get; set; }
        public int CurrentSalary { get; set; }
        public int Advance { get; set; }
        public string LastSalaryMonth { get; set; }
        public int LastSalaryAmount { get; set; }
        public int Year { get; set; }
        public int? Month { get; set; }
        public int Amount { get; set; }
        public bool AmountEditMode { get; set; }
        public int Paid { get; set; }
        public int Due { get; set; }
        public string Remarks { get; set; }
        public bool RemarksEditMode { get; set; }
        public decimal WorkingDays { get; set; }
        public int TotalDays { get; set; }
        public int Payable { get; set; }
        public bool FullPaid { get; set; }
    }
}
