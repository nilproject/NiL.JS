/// Copyright (c) 2012 Ecma International.  All rights reserved. 
/**
 * @path annexB/B.2.5.js
 * @description Object.getOwnPropertyDescriptor returns data desc for functions on built-ins (Date.prototype.setYear)
 */


function testcase() {
  var desc = Object.getOwnPropertyDescriptor(Date.prototype, "setYear");
  if (desc.value === Date.prototype.setYear &&
      desc.writable === true &&
      desc.enumerable === false &&
      desc.configurable === true) {
    return true;
  }
 }
runTestCase(testcase);
