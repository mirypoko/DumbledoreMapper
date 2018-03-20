using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleView
{
    public class Obj1
    {
        public int? Value1 { get; set; }

        public bool Value2 { get; set; }

        public int RandInt { get; set; }

        public Obj1()
        {
            RandInt = RandomUtil.GetRandomNumber(0,2);
            if (RandInt == 0)
            {
                Value1 = null;
            }
            else
            {
                Value1 = RandomUtil.GetRandomNumber(0, 99999);
            }
            Value2 = RandomUtil.GetRandomNumber(0, 2) == 0;
        }

        public override string ToString()
        {
            return "Obj1:\n" +
                   $"value1={Value1}\n" +
                   $"value2={Value2}\n" +
                   $"randInt={RandInt}\n";
        }
    }
}
