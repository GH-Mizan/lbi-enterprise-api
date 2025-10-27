using LbI.Enums;
using System;
using System.Collections.Generic;

namespace LbI.VirtualStocks.Dto
{
    public class VirtualStockEntryInput
    {
        public VirtualStockEntryDto Stock { get; set; }
        public List<VirtualStockDetailEntryDto> StockDetails { get; set; }
    }

    public class VirtualStockEntryDto
    {
        public int? Id { get; set; }
        public DateTime Date { get; set; }
        public int StockPointId { get; set; }
        public int ClientId { get; set; }
        public int SupervisorId { get; set; }
        public int DriverId { get; set; }
        public VirtualStockType VirtualStockType { get; set; }
    }

    public class VirtualStockDetailEntryDto
    {
        public int? Id { get; set; }
        public int VirtualStockId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int In { get; set; }
        public int Out { get; set; }
        public int StockQty { get; set; }
        public int InitialStockQty { get; set; }
        public bool Selected { get; set; }
    }
}
