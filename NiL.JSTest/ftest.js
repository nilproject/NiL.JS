
    function gNonStrict() {
        return gNonStrict.caller// || gNonStrict.caller.throwTypeError;
    }
    var f = new Function("\"use strict\";\nreturn gNonStrict();");
    f();