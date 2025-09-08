using LbI.Common;
using System;

namespace LbI.DailyCashes.Dto
{
    public class DailyCashFilterDto : FilterBaseDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
