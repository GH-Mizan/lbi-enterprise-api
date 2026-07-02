using LbI.AdditionalParties.Dto;
using System.Collections.Generic;

namespace LbI.Salaries.Dto
{
    public class SalaryAdvanceOutputDto
    {
       public List<SalaryAdvanceDto> Advances { get; set; }
        public List<AdditionalPartyOutputDto> AdditionalParitesAdvances { get; set; }
        public decimal TotalAdvance { get; set; }
       public decimal TotalLoan { get; set; }
       public decimal TotalBorrowing { get; set; }
       public decimal TotalAdditionalPartiesAdvance { get; set; }
    }
}
