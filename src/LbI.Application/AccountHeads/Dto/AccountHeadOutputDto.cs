using LbI.Enums;

namespace LbI.AccountHeads.Dto
{
    public class AccountHeadOutputDto
    {
        public int Id { get; set; }
        public ParentAccountHead ParentHead { get; set; }
        public string ParentName { get; set; }
        public string Name { get; set; }
        public AccountHeadType Type { get; set; }
        public string TypeText { get; set; }
    }
}
