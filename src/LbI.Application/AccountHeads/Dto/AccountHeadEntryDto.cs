using LbI.Enums;

namespace LbI.AccountHeads.Dto
{
    public class AccountHeadEntryDto
    {
        public int? Id { get; set; }
        public ParentAccountHead ParentHead { get; set; }
        public string Name { get; set; }
        public AccountHeadType Type { get; set; }
    }
}
