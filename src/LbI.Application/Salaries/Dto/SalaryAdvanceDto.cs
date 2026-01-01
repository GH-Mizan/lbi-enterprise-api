using System;

namespace LbI.Salaries.Dto
{
    public class SalaryAdvanceDto
    {
        public int? Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int PreviousSalary { get; set; }
        public bool PreviousSalaryEditMode { get; set; }
        public int CurrentSalary { get; set; }
        public bool CurrentSalaryEditMode { get; set; }
        public DateTime? IncrementDate { get; set; }
        public int Advance { get; set; }
        public bool AdvanceEditMode { get; set; }
        public int LoanToCompany { get; set; }
        public bool LoanToEditMode { get; set; }
        public int LoanFromCompany { get; set; }
        public bool LoanFromEditMode { get; set; }
        public string Remarks { get; set; }
        public bool RemarksEditMode { get; set; }
        public bool FromUi { get; set; }
    }
}
