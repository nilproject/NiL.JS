using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NiL.JS.Core;

namespace NetCoreTestApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Context.GlobalContext.DefineVariable("console").Assign(JSValue.Marshal(new
            {
                log = new Action<object>((x) => Console.WriteLine(x))
            }));

            new Context().Eval("console.log('hello')");
        }
    }
}
