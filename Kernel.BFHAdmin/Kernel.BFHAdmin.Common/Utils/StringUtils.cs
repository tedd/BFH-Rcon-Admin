using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.BFHAdmin.Common.Utils
{
    public static class StringUtils
    {
        public static string ListConcatWithAnd(IEnumerable<string> list)
        {
            var myList = new List<string>(list);
            string str = null;
            var last = myList.Last();
            myList.Remove(last);
            if (myList.Count > 0)
                str = String.Join(", ", myList);
            if (str != null)
                str += " & ";
            str += last;
            return str;
        }
    }
}
