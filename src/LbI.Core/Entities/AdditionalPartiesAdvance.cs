using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace LbI.Entities
{
    public class AdditionalPartiesAdvance : FullAuditedEntity
    {
        public string PartyName { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Relation { get; set; }
        public int Advance { get; set; }
        public string Notes { get; set; }
    }
}
