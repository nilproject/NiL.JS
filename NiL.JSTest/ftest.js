function test(x) {
    (function () {
        var a = 1;
        if (x > 0)
            test(x - 1);
        return (x, a);
    })();
}
test(1);