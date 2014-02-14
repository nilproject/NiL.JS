var match = String.prototype.match;

if (typeof toString === "undefined") {
    toString = Object.prototype.toString;
}

var __class__ = toString();

//////////////////////////////////////////////////////////////////////////////
//CHECK#1
if (match(eval("\"bj\""))[0] !== "bj") {
    $ERROR('#1: match = String.prototype.match; match(eval("\\"bj\\""))[0] === "bj". Actual: ' + match(eval("\"bj\""))[0]);
}