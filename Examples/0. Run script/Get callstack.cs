using ExamplesFramework;
using NiL.JS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examples.Run_script
{
    [Level(0)]
    public sealed class Get_stack : Example
    {
        private static readonly string _code = @"

function test1() {
    test2();
}

function test2() {
    test3();
}

function test3() {
    test4();
}

function test4() {
    var x = new MissingThing(1, 2, 3);
}

try {
    test1();
}
catch (e) {
    try {
        console.log(e.toString());
        var x = new MissingThing(1, 1, 1);
    }
    catch (ex) {
        console.log(ex.toString());
    }
}

";
        public override void Run()
        {

            Script.Parse(_code).Evaluate(new NiL.JS.Core.Context());
        }
    }
}
