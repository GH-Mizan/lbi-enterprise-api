using LbI.Enums;

namespace LbI.VirtualItems.Dto
{
    public class VirtualItemEntryInput
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public ProductType Type { get; set; }
        public bool ActiveStatus { get; set; }
    }
}
