$ERROR = console.log;
$FAIL = console.log;
console.log(function () {
    var arrObj = [0, 1, 2];
    var arrProtoLen;

    try {
        arrProtoLen = Array.prototype.length;
        Array.prototype.length = 0;


        console.log(Array.prototype.length);
        Object.defineProperty(arrObj, "2", {
            configurable: false
        });
        console.log(Array.prototype.length);

        Object.defineProperty(arrObj, "length", {
            value: 1
        });
        return false;
    } catch (e) {
        console.log(Array.prototype);
        console.log(arrObj.length);
        console.log(e);
        return e instanceof TypeError && arrObj.length === 3 && Array.prototype.length === 0;
    } finally {
        Array.prototype.length = arrProtoLen;
    }
}());