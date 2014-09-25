/// Copyright (c) 2012 Ecma International.  All rights reserved. 
/**
 * @path annexB/B.2.3.js
 * @description Object.getOwnPropertyDescriptor returns data desc for functions on built-ins (String.prototype.substr)
 */


function testcase() {
  var desc = Object.getOwnPropertyDescriptor(String.prototype, "substr");
  if (desc.value === String.prototype.substr &&
      desc.writable === true &&
      desc.enumerable === false &&
      desc.configurable === true) {
    return true;
  }
 }
runTestCase(testcase);
