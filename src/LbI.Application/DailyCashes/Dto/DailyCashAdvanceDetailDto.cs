namespace LbI.DailyCashes.Dto
{
    public class DailyCashAdvanceDetailDto
    {
        public int Id { get; set; }
        public int DailyCashId { get; set; }
        public int EmployeeId { get; set; }
        public string Type { get; set; }
        public string Head { get; set; }
        public decimal Amount { get; set; }
        public string Uid { get; set; }
    }
}
