using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;

namespace LbI.Entities
{
    public class Customer : FullAuditedEntity
    {
        public required string Name { get; set; }
        public required string ShortName { get; set; }
        public string Address { get; set; }
        public string ContactNo { get; set; }
        public string Email { get; set; }
        public bool ActiveStatus { get; set; }
        public string Remarks { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal InitialDue { get; set; }
    }
}
