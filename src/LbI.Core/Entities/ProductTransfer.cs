using Abp.Domain.Entities.Auditing;

namespace LbI.Entities
{
    public class ProductTransfer : FullAuditedEntity
    {
        public int ProductId { get; set; }
        public int FromStockPointId { get; set; }
        public int TransferQuantity { get; set; }
        public int ToStockPointId { get; set; }
    }
}
