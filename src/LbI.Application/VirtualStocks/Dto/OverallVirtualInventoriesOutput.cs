using LbI.Enums;

namespace LbI.VirtualStocks.Dto
{
    public class OverallVirtualInventoriesOutput
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public VirtualStockType VirtualStockType { get; set; }
        public int StockQty { get; set; }
    }
}
