console.log(function () {
    var proto = {};

    Object.defineProperty(proto, "foo", {
        get: function () {
            return 0;
        },
        configurable: true
    });

    var ConstructFun = function () { };
    ConstructFun.prototype = proto;

    var child = new ConstructFun();
    Object.defineProperty(child, "foo", {
        value: 10,
        configurable: true
    });
    var preCheck = Object.isExtensible(child);
    Object.seal(child);

    delete child.foo;
    return preCheck && child.foo === 10;
}());