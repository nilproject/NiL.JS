var $ERROR = console.log;

console.log(function () {
    var arrObj = [];
    function getFunc() {
        return 12;
    }

    Object.defineProperty(arrObj, "1", {
        get: getFunc
    });

    try {
        Object.defineProperty(arrObj, "1", {
            get: undefined
        });
        return false;
    } catch (e) {
        var hasProperty = arrObj.hasOwnProperty("1");
        var desc = Object.getOwnPropertyDescriptor(arrObj, "1");

        var verifyGet = arrObj[1] === getFunc();

        var verifySet = desc.hasOwnProperty("set") && typeof desc.set === "undefined";

        var verifyEnumerable = false;
        for (var p in arrObj) {
            if (p === "1") {
                verifyEnumerable = true
            }
        }

        var verifyConfigurable = false;
        delete arrObj[1];
        verifyConfigurable = arrObj.hasOwnProperty("1");

        console.log(e instanceof TypeError);
        console.log(hasProperty);
        console.log(verifyGet);
        console.log(verifySet);
        console.log(!verifyEnumerable);
        console.log(verifyConfigurable);

        return e instanceof TypeError && hasProperty && verifyGet &&
            verifySet && !verifyEnumerable && verifyConfigurable;
    }
}());