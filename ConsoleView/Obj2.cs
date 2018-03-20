using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleView
{
    public class Obj2
    {
        public int Value1 { get; set; }

        public bool? Value2 { get; set; }

        public Obj2()
        {
            if (RandomUtil.GetRandomNumber(0, 2) == 0)
            {
                Value2 = null;
            }
            else
            {
                Value2 = RandomUtil.GetRandomNumber(0, 2) == 0;
            }

            Value1 = RandomUtil.GetRandomNumber(-9999, 0);
        }
        public override string ToString()
        {
            return "Obj2:\n" +
                   $"value1={Value1}\n" +
                   $"value2={Value2}\n";
        }
    }
}
