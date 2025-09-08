using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;

namespace LbI.Entities
{
    public class PurchaseDetail : FullAuditedEntity
    {
        public int PurchaseId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalPrice { get; set; }
        public string Remarks { get; set; }
    }
}
