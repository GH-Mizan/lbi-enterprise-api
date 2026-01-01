using System;

namespace LbI.Salaries.Dto
{
    public class SalaryEntryInputDto
    {
        public  int? Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Amount { get; set; }
        public decimal WorkingDays { get; set; }
        public int Payable { get; set; }
        public bool FullPaid { get; set; }
        public string Remarks { get; set; }
    }
}
