console.log((function () {
    var arr = { length: 30 };
    var targetObj = function () { };

    var fromIndex = {
        valueOf: function () {
            arr[4] = targetObj;
            return 3;
        }
    };

    return 4 === Array.prototype.indexOf.call(arr, targetObj, fromIndex);
})());