using System;

namespace LbI.Inventories.Dto
{
    public class ProductTransferEntryDto
    {
        public int? Id { get; set; }
        public DateTime TransferDate { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int FromStockPointId { get; set; }
        public int TransferQuantity { get; set; }
        public int ToStockPointId { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
