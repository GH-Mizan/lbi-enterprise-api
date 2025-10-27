using Abp.Domain.Entities.Auditing;
using LbI.Enums;
using System;

namespace LbI.Entities
{
    public class VirtualStock : FullAuditedEntity
    {
        public DateTime Date { get; set; }
        public int StockPointId { get; set; }
        public int ClientId { get; set; }
        public int SupervisorId { get; set; }
        public int DriverId { get; set; }
        public VirtualStockType VirtualStockType { get; set; }
    }
}
