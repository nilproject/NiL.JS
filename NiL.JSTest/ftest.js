var $ERROR = console.log;

console.log(function () {
    "use strict";

    var argObj = function () {
        return arguments;
    }();

    var verifyEnumerable = false;
    for (var _10_6_14_b_1 in argObj) {
        if (argObj.hasOwnProperty(_10_6_14_b_1) && _10_6_14_b_1 === "caller") {
            verifyEnumerable = true;
        }
    }
    return !verifyEnumerable && argObj.hasOwnProperty("caller");
}());