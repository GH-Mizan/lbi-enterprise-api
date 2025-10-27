using Abp.Domain.Entities.Auditing;
using LbI.Enums;

namespace LbI.Entities
{
    public class VirtualItem : FullAuditedEntity
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public ProductType Type { get; set; }
        public  bool ActiveStatus { get; set; }
    }
}
