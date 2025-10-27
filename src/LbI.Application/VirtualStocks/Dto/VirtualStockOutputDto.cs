using LbI.Enums;
using System;

namespace LbI.VirtualStocks.Dto
{
    public class VirtualStockOutputDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int StockPointId { get; set; }
        public string StockPoint { get; set; }
        public int Oxygen136In { get; set; }
        public int Oxygen136Out { get; set; }
        public int Oxygen136Stock { get; set; }
        public int Oxygen98In { get; set; }
        public int Oxygen98Out { get; set; }
        public int Oxygen98Stock { get; set; }
        public int MedicalAirIn { get; set; }
        public int MedicalAirOut { get; set; }
        public int MedicalAirStock { get; set; }
        public int NitrousIn { get; set; }
        public int NitrousOut { get; set; }
        public int NitrousStock { get; set; }

        public int SupervisorId { get; set; }
        public int DriverId { get; set; }
        public VirtualStockType VirtualStockType { get; set; }
    }
}
