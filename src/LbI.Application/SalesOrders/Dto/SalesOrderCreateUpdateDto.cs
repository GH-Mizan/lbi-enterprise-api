using System;

namespace LbI.SalesOrders.Dto
{
    public class SalesOrderCreateUpdateDto
    {
        public int? Id { get; set; }
        public DateTime Date { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int NitrousQty { get; set; }
        public int AirQty { get; set; }
        public int Oxygen136Qty { get; set; }
        public int Oxygen980Qty { get; set; }
    }
}
