using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LbI.Helpers
{
    public static class EnumHelper
    {
        public static string DisplayName(this Enum enumValue)
        {
            try
            {
                var attributes = (DescriptionAttribute[])enumValue.GetType().GetField(enumValue.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);

                return attributes.Length > 0 ? attributes[0].Description : enumValue.ToString().PascalCaseToPrettyString();
            }
            catch (Exception)
            {
                return enumValue.ToString().PascalCaseToPrettyString();
            }
        }



        public static string PascalCaseToPrettyString(this string s)
        {
            return Regex.Replace(s, @"(\B[A-Z]|[0-9]+)", " $1");
        }

    }
}
