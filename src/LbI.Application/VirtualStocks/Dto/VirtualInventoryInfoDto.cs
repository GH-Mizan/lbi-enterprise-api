using System;
using System.Collections.Generic;

namespace LbI.VirtualStocks.Dto
{
    public class VirtualInventoryInfoDto
    {
        public DateTime? LastDate { get; set; }
        public List<VirtualInventoryDto> Inventories { get; set; }
    }
    public class VirtualInventoryDto
    {
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public int StockQty { get; set; }
    }
}
