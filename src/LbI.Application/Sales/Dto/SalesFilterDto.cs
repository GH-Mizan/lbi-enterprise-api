using LbI.Common;
using System;

namespace LbI.Sales.Dto
{
    public class SalesFilterDto : FilterBaseDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
