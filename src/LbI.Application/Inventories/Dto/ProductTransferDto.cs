using System;

namespace LbI.Inventories.Dto
{
    public class ProductTransferDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int FromStockPointId { get; set; }
        public string FromStockPointName { get; set; }
        public int TransferQuantity { get; set; }
        public int ToStockPointId { get; set; }
        public string ToStockPointName { get; set; }
        public DateTime CreationTime { get; set; }

    }
}
