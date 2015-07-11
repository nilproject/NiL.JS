console.log(function () {
    var obj = { length: 1 };

    try {
        Object.prototype[0] = false;
        Object.defineProperty(obj, "0", {
            get: function () {
                return true;
            },
            configurable: true
        });

        return 0 === Array.prototype.indexOf.call(obj, true);
    } finally {
        delete Object.prototype[0];
    }
}());