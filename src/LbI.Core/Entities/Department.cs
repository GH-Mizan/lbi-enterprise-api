using Abp.Domain.Entities.Auditing;

namespace LbI.Entities
{
    public class Department : FullAuditedEntity
    {
        public string Name { get; set; }
        public bool ActiveStatus { get; set; }
    }
}
