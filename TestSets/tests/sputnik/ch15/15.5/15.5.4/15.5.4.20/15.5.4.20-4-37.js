/// Copyright (c) 2012 Ecma International.  All rights reserved. 
/**
 * @path ch15/15.5/15.5.4/15.5.4.20/15.5.4.20-4-37.js
 * @description String.prototype.trim handles whitepace and lineterminators (ab\u000Cc)
 */


function testcase() {
  if ("ab\u000Cc".trim() === "ab\u000Cc") {
    return true;
  }
 }
runTestCase(testcase);
