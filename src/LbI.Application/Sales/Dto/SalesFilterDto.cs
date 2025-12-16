using LbI.Common;
using System;

namespace LbI.Sales.Dto
{
    public class SalesFilterDto : FilterBaseDto
    {
        public DateTime? Date { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
