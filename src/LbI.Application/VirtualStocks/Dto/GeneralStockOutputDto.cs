using LbI.Enums;
using System.Collections.Generic;

namespace LbI.VirtualStocks.Dto
{
    public class GeneralStockOutputDto
    {
        public int Oxygen136Total { get; set; }
        public int Oxygen98Total { get; set; }
        public int MedicalAirTotal { get; set; }
        public int NitrousOxideTotal { get; set; }
        public int GrandTotal { get; set; }
        public List<GeneralStockDetailsDto> Details { get; set; }
    }

    public class GeneralStockDetailsDto()
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int Oxygen136 { get; set; }
        public int Oxygen98 { get; set; }
        public int MedicalAir { get; set; }
        public int NitrousOxide { get; set; }
        public int Total { get; set; }
        public VirtualStockType VirtualStockType { get; set; }
    }
}
