/*function fib(n) {
    if (n <= 2) {
        return n;
    }
    return fib(n - 1) + fib(n - 2);
}
var start = new Date;
for (var i = 0; i < 35; i++)
    fib(i);
console.log(new Date - start);
*/

var o = { v1: 1, v2: 2, "2": "index", i: 0 };
for (o.i in [1,2,3])
    console.log(o.i);