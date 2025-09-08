using System.ComponentModel;

namespace LbI.Enums
{
    public enum PaymentStatus
    {
        Paid = 1,
        [Description("Partial Paid")]
        Partialpaid,
        Due
    }
}
