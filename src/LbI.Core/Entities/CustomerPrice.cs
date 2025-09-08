using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;

namespace LbI.Entities
{
    public class CustomerPrice : FullAuditedEntity
    {
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Price { get; set; }
    }
}
