var $ERROR = console.log;

console.log(function () {
    var arr = { length: 2 };

    Object.defineProperty(arr, "1", {
        get: function () {
            return 6.99;
        },
        configurable: true
    });

    Object.defineProperty(arr, "0", {
        get: function () {
            delete arr[1];
            return 0;
        },
        configurable: true
    });

    return -1 === Array.prototype.indexOf.call(arr, 6.99);
}());