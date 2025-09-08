using LbI.Common;
using System;

namespace LbI.Purchases.Dto
{
    public class PurchasesFilterDto : FilterBaseDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
