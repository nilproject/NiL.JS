/// Copyright (c) 2012 Ecma International.  All rights reserved. 
/**
 * @path annexB/B.2.1.js
 * @description Object.getOwnPropertyDescriptor returns data desc for functions on built-ins (Global.escape)
 */


function testcase() {
    var global = fnGlobalObject();
    var desc = Object.getOwnPropertyDescriptor(global, "escape");
    if (desc.value === global.escape &&
        desc.writable === true &&
        desc.enumerable === false &&
        desc.configurable === true) {
        return true;
    }
}
runTestCase(testcase);