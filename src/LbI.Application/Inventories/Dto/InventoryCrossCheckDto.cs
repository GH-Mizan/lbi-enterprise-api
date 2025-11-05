namespace LbI.Inventories.Dto
{
    public class InventoryCrossCheckDto
    {
        public string ProductName { get; set; }
        public int ActualStockQty { get; set; }
        public int VirtualStockQty { get; set; }
        public int Difference { get; set; }
        public bool HasDifference { get; set; }
    }
}
