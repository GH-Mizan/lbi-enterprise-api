using LbI.Enums;

namespace LbI.VirtualStocks.Dto
{
    public class PlantWarehouseSelectListDto
    {
        public int Uid { get; set; }
        public int Id { get; set; }
        public string DisplayText { get; set; }
        public VirtualStockType VirtualStockType { get; set; }
    }
}
