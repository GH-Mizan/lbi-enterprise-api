using Abp.Domain.Entities.Auditing;
using LbI.Enums;

namespace LbI.Entities
{
    public class AccountHead : FullAuditedEntity
    {
        public ParentAccountHead ParentHead { get; set; }
        public string Name { get; set; }
        public AccountHeadType Type { get; set; }
    }
}
