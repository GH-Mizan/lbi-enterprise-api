using LbI.Enums;

namespace LbI.Sales.Dto
{
    public class SalesProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public ProductType Type { get; set; }
        public string TypeText { get; set; }
        public string Size { get; set; }
        public decimal SalesPrice { get; set; }
        public bool SalesPriceDisabled { get; set; }
        public bool Selected { get; set; }
        public int Quantity { get; set; }
        public int? Stock { get; set; }
        public bool QtyDisabled { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
