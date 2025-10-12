namespace LbI.Inventories.Dto
{
    public class InventoryOutputDto
    {
        public int Serial { get; set; }
        public int StockPointId { get; set; }
        public string StockPointName { get; set; }
        public int ProductId { get; set; }
        public int MedicalOxygen9_8Qty { get; set; }
        public int MedicalOxygen1_36Qty { get; set; }
        public int MedicalAir9_8Qty { get; set; }
        public int MedicalAir7Qty { get; set; }
        public int Nitros30KgQty { get; set; }
        public int Nitros5KgQty { get; set; }
        public int Nitros3KgQty { get; set; }
        public int Total { get; set; }
    }
    
}
