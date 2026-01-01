namespace LbI.Purchases.Dto
{
    public class BuyAndSalesDifferenceDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Purchase { get; set; }
        public int Sales { get; set; }
        public int Difference { get; set; }
    }
}
