function foo(a) {
    var f = function bar(b) {
        if (a == 1)
            foo(0);

        if (b == 1)
            bar(0);
    }

    var bar = 'expected result';
    f(1);
    eval('');
    return bar;
}

var f = foo(1);

console.assert(f == 'expected result');