namespace LbI.Inventories.Dto
{
    public class StockQuantityOutputDto
    {
        public int StockPointId { get; set; }
        public string StockPointName { get; set; }
        public int Stock { get; set; }
    }

    public class StockWiseInventoryOutputDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int StockPointId { get; set; }
        public string StockPointName { get; set; }
        public int Stock { get; set; }
    }
}
