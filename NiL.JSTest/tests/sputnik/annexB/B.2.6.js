/// Copyright (c) 2012 Ecma International.  All rights reserved. 
/**
 * @path annexB/B.2.6.js
 * @description Object.getOwnPropertyDescriptor returns data desc for functions on built-ins (Date.prototype.toGMTString)
 */


function testcase() {
  var desc = Object.getOwnPropertyDescriptor(Date.prototype, "toGMTString");
  if (desc.value === Date.prototype.toGMTString &&
      desc.writable === true &&
      desc.enumerable === false &&
      desc.configurable === true) {
    return true;
  }
 }
runTestCase(testcase);
