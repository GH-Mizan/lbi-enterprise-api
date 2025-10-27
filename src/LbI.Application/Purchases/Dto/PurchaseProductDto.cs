using LbI.Enums;

namespace LbI.Purchases.Dto
{
    public class PurchaseProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public ProductType Type { get; set; }
        public string TypeText { get; set; }
        public ProductSize Size { get; set; }
        public string SizeText { get; set; }
        public decimal PurchasePrice { get; set; }
        public bool PurchasePriceDisabled { get; set; }
        public bool Selected { get; set; }
        public int Quantity { get; set; }
        public int? Stock { get; set; }
        public bool QtyDisabled { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
