using System;
using System.Collections.Generic;

namespace LbI.DailyCashes.Dto
{
    public class VouchersAndDailyCashCreateUpdateDto
    {
        public DateTime Date { get; set; }
        public List<VoucherEntryDto> Vouchers { get; set; }
        public CreateOrUpdateDailyCashInput DailyCash { get; set; }
    }
}
