console.log((function () {
    (function (x) {
        if (x >= 0) return; return arguments.callee(x + 1);
    })(-5);
    function foo() { }
    var obj = foo.bind({});
    return obj.hasOwnProperty("arguments");
})());