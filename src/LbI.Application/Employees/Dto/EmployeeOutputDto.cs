using System;

namespace LbI.Employees.Dto
{
    public class EmployeeOutputDto
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int DesignationId { get; set; }
        public string DesignationName { get; set; }
        public DateTime JoiningDate { get; set; }
        public DateTime BirthDate { get; set; }
        public string BloodGroup { get; set; }
        public string ContactNumber { get; set; }
        public string Address { get; set; }
        public string Reference { get; set; }
        public string Remarks { get; set; }
        public bool ActiveStatus { get; set; }
    }
}
