using Abp.Domain.Entities.Auditing;

namespace LbI.Entities
{
    public class Designation : FullAuditedEntity
    {
        public string Title { get; set; }
        public bool ActiveStatus { get; set; }
    }
}
