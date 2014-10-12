var t0 = __pinvoke(function () { for (var i = 0; i < 10000; i++) console.log("1        2"); }) // parallel invoke
var t1 = __pinvoke(function () { for (var i = 0; i < 10000; i++) console.log("2        1"); })
t0.wait();
t1.wait();