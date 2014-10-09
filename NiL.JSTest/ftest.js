$ERROR = console.log;
$FAIL = console.log;
console.log(function () {
    var gen = (function* (iterations) { while(iterations-->0) yield iterations; });
        for(var g = gen(100); !g.next().done;)
            console.log("1234");
        
}());