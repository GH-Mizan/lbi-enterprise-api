using Abp.Domain.Entities.Auditing;
using System;

namespace LbI.Entities
{
    public class SalaryAdvance : FullAuditedEntity
    {
        public int EmployeeId { get; set; }
        public int PreviousSalary { get; set; }
        public int CurrentSalary { get; set; }
        public DateTime? IncrementDate { get; set; }
        public int Advance { get; set; }
        public int LoanToCompany { get; set; }
        public int LoanFromCompany { get; set; }
        public string Remarks { get; set; }
        public bool FromUi { get; set; }
    }
}
