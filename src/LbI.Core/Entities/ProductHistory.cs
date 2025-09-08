using Abp.Domain.Entities.Auditing;
using LbI.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace LbI.Entities
{
    public class ProductHistory : FullAuditedEntity
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public ProductType Type { get; set; }
        public string Size { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal PurchasePrice { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal SellPrice { get; set; }
        public bool ActiveStatus { get; set; }
    }
}
