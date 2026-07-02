namespace LbI.DailyCashes.Dto
{
    public class DailyCashIncomeDetailDto
    {
        public int? Id { get; set; }
        public int DailyCashId { get; set; }
        public int IncomeId { get; set; }
        public string Key { get; set; }
        public string Head { get; set; }
        public decimal Amount { get; set; }
        public string MtmKey { get; set; }
        public string Uid { get; set; }
    }
}
