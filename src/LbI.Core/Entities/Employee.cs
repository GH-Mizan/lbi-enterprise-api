using Abp.Domain.Entities.Auditing;
using System;

namespace LbI.Entities
{
    public class Employee : FullAuditedEntity
    {
        public string Name { get; set; }
        public int DepartmentId { get; set; }
        public int DesignationId { get; set; }
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
