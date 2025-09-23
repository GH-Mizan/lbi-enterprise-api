using LbI.Enums;

namespace LbI.LbiSettings.Dto
{
    public class LbiSettingsOutputDto
    {
        public  int Id { get; set; }
        public InitialSetupKey Key { get; set; }
        public string KeyText { get; set; }
        public string Value { get; set; }
    }
}
