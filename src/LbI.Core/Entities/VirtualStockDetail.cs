using Abp.Domain.Entities.Auditing;

namespace LbI.Entities
{
    public class VirtualStockDetail : FullAuditedEntity
    {
        public int VirtualStockId { get; set; }
        public int ProductId { get; set; }
        public int In { get; set; }
        public int Out { get; set; }
        public int StockQty { get; set; }
    }
}

