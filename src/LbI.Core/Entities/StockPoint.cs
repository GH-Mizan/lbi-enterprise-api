using Abp.Domain.Entities.Auditing;
using LbI.Enums;

namespace LbI.Entities
{
    public class StockPoint : FullAuditedEntity
    {
        public string Name { get; set; }
        public StockPointType StockPointType { get; set; }
        public string StockPointNumber { get; set; }
        public string GpsTrackerNo { get; set; }
        public bool ActiveStatus { get; set; }
        public string Remarks { get; set; }
    }
}
