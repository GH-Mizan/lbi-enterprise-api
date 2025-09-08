using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LbI.Common
{
    public class FilterBaseDto
    {
        public string SearchText { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}
