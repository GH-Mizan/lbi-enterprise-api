namespace LbI.Purchases.Dto
{
    public class MonthlyPurchaseReportDto
    {
        public string Serial { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public decimal Amount { get; set; }

        public int MedicalOxygen9_8Qty { get; set; }
        public int MedicalOxygen1_36Qty { get; set; }
        public int MedicalAir9_8Qty { get; set; }
        public int MedicalAir7Qty { get; set; }
        public int Nitros30KgQty { get; set; }
        public int Nitros5KgQty { get; set; }
        public int Nitros3KgQty { get; set; }
    }
}
