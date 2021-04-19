/// Copyright (c) 2012 Ecma International.  All rights reserved. 
/**
 * @path annexB/B.2.4.js
 * @description Object.getOwnPropertyDescriptor returns data desc for functions on built-ins (Date.prototype.getYear)
 */


function testcase() {
  var desc = Object.getOwnPropertyDescriptor(Date.prototype, "getYear");
  if (desc.value === Date.prototype.getYear &&
      desc.writable === true &&
      desc.enumerable === false &&
      desc.configurable === true) {
    return true;
  }
 }
runTestCase(testcase);
