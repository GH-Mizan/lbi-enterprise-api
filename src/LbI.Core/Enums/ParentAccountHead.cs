using System.ComponentModel;

namespace LbI.Enums
{
    public enum ParentAccountHead
    {
        Collection = 1,
        Loan,
        [Description("Others Income")]
        OthersIncome,
        [Description("Major Expense")]
        MajorExpense,
        [Description("Administrative Expense")]
        AdministrativeExpense,
        [Description("Delivery Expense")]
        DeliveryExpense,
        [Description("Cylinder Preparation Expense")]
        CylinderPreparationExpense, 
        [Description("Vehicle Expense")]
        VehicleExpense

    }
}
