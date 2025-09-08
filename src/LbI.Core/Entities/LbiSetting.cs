using Abp.Domain.Entities.Auditing;
using LbI.Enums;

namespace LbI.Entities
{
    public class LbiSetting : FullAuditedEntity
    {
        public InitialSetupKey Key { get; set; }
        public string Value { get; set; }
    }
}
