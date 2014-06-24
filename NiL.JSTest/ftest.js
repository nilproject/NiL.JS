function f1(func, step)
{
    a = 1;
    if (step == 1)
        return function () { return a; }
    do {
        var a = 2;
    } while (false);
    console.log(func());
}
var f = f1(null, 1);
console.log(f());
f1(f, 2);