console.log((function (a) {
    function f(o) {

        function innerf(o, x) {
            with (o) {
                return x;
            }
        }

        return innerf(o, 42);
    }

    if (f({}) === 42) {
        return true;
    }
})());