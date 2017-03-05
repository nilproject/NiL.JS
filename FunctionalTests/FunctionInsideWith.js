with ({ a: 'hello' }) {
    function f() {
        (function () { })(a);
    }

    f();
}