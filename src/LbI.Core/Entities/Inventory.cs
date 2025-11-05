using Abp.Domain.Entities.Auditing;

namespace LbI.Entities
{
    public class Inventory : FullAuditedEntity
    {
        public int ProductId { get; set; }
        public int StockPointId { get; set; }
        public int StockQty { get; set; }
        public bool Damadged { get; set; }
    }
}
