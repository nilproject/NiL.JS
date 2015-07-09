console.log(function () {
    function callbackfn(prevVal, curVal, idx, obj) {
        return obj instanceof Boolean;
    }

    var obj = new Boolean(true);
    obj.length = 2;
    obj[0] = 11;
    obj[1] = 12;
    console.log(obj.length);
    console.log(obj[0]);
    console.log(obj[1]);
    return Array.prototype.reduce.call(obj, callbackfn, 1);
}());