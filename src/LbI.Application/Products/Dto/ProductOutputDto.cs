using LbI.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace LbI.Products.Dto
{
    public class ProductOutputDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ProductType Type { get; set; }
        public string TypeText { get; set; }
        public string Size { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SellPrice { get; set; }
        public bool ActiveStatus { get;     set; }
    }
}
