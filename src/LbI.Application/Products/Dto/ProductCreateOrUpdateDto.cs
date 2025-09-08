using LbI.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace LbI.Products.Dto
{
    public class ProductCreateOrUpdateDto
    {
        public int? Id { get; set; }
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
