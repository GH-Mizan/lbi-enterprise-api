using Abp.Domain.Entities.Auditing;
using LbI.Enums;

namespace LbI.Entities
{
    public class VirtualInventory : FullAuditedEntity
    {
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public int StockQty { get; set; }
        public VirtualStockType VirtualStockType { get; set; }
    }
}
