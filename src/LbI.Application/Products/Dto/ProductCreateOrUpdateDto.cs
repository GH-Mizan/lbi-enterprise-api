using LbI.Enums;

namespace LbI.Products.Dto
{
    public class ProductCreateOrUpdateDto
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public ProductType Type { get; set; }
        public ProductSize Size { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SellPrice { get; set; }
        public bool ActiveStatus { get; set; }
    }
}
