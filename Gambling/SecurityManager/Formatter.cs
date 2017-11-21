using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityManager
{
    public class Formatter
    {
        public static string GetName(string name)
        {
            string[] array = name.Split('\\');
            if (array.Length > 1)
            {
                return array[1];
            }
            else
            {
                array = name.Split('@');
                return array[0];
            }
        }
    }
}
