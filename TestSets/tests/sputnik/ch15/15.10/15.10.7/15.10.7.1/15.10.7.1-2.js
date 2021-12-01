/// Copyright (c) 2012 Ecma International.  All rights reserved. 
/**
 * @path ch15/15.10/15.10.7/15.10.7.1/15.10.7.1-2.js
 * @description RegExp.prototype.source is a data property with default attribute values (false)
 * 
 * @ignore
 */


function testcase() {
  var d = Object.getOwnPropertyDescriptor(RegExp.prototype, 'source');
  
  if (d.writable === false &&
      d.enumerable === false &&
      d.configurable === false) {
    return true;
  }
 }
runTestCase(testcase);
