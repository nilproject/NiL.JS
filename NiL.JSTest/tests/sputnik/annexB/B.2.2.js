/// Copyright (c) 2012 Ecma International.  All rights reserved. 
/**
 * @path annexB/B.2.2.js
 * @description Object.getOwnPropertyDescriptor returns data desc for functions on built-ins (Global.unescape)
 */


function testcase() {
  var global = fnGlobalObject();
  var desc = Object.getOwnPropertyDescriptor(global,  "unescape");
  if (desc.value === global.unescape &&
      desc.writable === true &&
      desc.enumerable === false &&
      desc.configurable === true) {
    return true;
  }
 }
runTestCase(testcase);
