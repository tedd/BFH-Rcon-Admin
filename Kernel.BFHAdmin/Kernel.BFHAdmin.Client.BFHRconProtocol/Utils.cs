using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.BFHAdmin.Client.BFHRconProtocol
{
    public class Utils
    {
        public static readonly char[] Tab = "\t".ToCharArray();
        public static bool BoolParse(string boolToParse)
        {
            if (boolToParse == "1")
                return true;
            if (boolToParse == "0")
                return false;
            return bool.Parse(boolToParse);

        }
    }
}
