using Abp.Domain.Entities.Auditing;
using System;

namespace LbI.Entities
{
    public class SalesOrder : FullAuditedEntity
    {
        public DateTime Date { get; set; }
        public int CustomerId { get; set; }
        public int NitrousQty { get; set; }
        public int AirQty { get; set; }
        public int Oxygen136Qty { get; set; }
        public int Oxygen980Qty { get; set; }
    }
}
