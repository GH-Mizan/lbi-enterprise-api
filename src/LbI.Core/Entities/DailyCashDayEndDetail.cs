using Abp.Domain.Entities.Auditing;

namespace LbI.Entities
{
    public class DailyCashDayEndDetail : FullAuditedEntity
    {
        public int DailyCashId { get; set; }
        public int EmployeeId { get; set; }
        public string Type { get; set; }
        public string Head { get; set; }
        public decimal Amount { get; set; }
        public string MtmKey { get; set; }
        public string Uid { get; set; }
    }
}
