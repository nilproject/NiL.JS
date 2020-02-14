using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExamplesFramework;
using NiL.JS;

namespace Examples.Using_modules
{
    [Level(1)]
    public sealed class Execution_time_limit : Example
    {
        public override void Run()
        {
            var module = new Module(@"for (;;) ;");

            var stopWatch = Stopwatch.StartNew();

            Console.WriteLine("Going to sleep");

            try
            {
                module.Run(timeLimitInMilliseconds: 3000);
            }
            catch(TimeoutException)
            {
                Console.WriteLine("Time is over");
            }

            Console.WriteLine("Wake up!");

            stopWatch.Stop();
            Console.WriteLine("Sleep time: " + stopWatch.Elapsed);
        }
    }
}
