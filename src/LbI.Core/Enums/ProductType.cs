using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LbI.Enums
{
    public enum ProductType
    {
        [Description("Medical Oxygen")]
        MedicalOxygen = 1,
        [Description("Medical Air")]
        MedicalAir,
        [Description("Nitrous")]
        Nitrous
    }
}
