/// Copyright (c) 2012 Ecma International.  All rights reserved. 
/**
 * @path test/suite/annexB/B.RegExp.prototype.compile.js
 * @description Object.getOwnPropertyDescriptor returns data desc for functions on built-ins (RegExp.prototype.compile)
 */


function testcase() {
  var desc = Object.getOwnPropertyDescriptor(RegExp.prototype, "compile");
  if (desc.value === RegExp.prototype.compile &&
      desc.writable === true &&
      desc.enumerable === false &&
      desc.configurable === true) {
    return true;
  }
 }
runTestCase(testcase);
