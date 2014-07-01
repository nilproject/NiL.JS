console.log((function () {
    return (function (a, b, c) {
        Object.defineProperty(arguments, "0", {
            value: 20,
            writable: false,
            enumerable: false,
            configurable: false
        });
        var verifyFormal = a === 20;
        //return Object.getOwnPropertyDescriptor(arguments, "0");
        return dataPropertyAttributesAreCorrect(arguments, "0", 20, false, false, false) && verifyFormal;
    }(0, 1, 2));
})());