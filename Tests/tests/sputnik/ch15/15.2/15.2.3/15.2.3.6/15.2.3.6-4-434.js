/// Copyright (c) 2012 Ecma International.  All rights reserved. 
/**
 * @path ch15/15.2/15.2.3/15.2.3.6/15.2.3.6-4-434.js
 * @description ES5 Attributes - property ([[Get]] is undefined, [[Set]] is undefined, [[Enumerable]] is true, [[Configurable]] is false) is undeletable
 */


function testcase() {
        var obj = {};

        Object.defineProperty(obj, "prop", {
            get: undefined,
            set: undefined,
            enumerable: true,
            configurable: false
        });

        var propertyDefineCorrect = obj.hasOwnProperty("prop");
        var desc = Object.getOwnPropertyDescriptor(obj, "prop");

        delete obj.prop;

        return propertyDefineCorrect && desc.configurable === false && obj.hasOwnProperty("prop");
    }
runTestCase(testcase);