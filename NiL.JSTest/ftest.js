
protoObj = {};
//Establish foo object
function FooObj() { };

//////////////////////////////////////////////////////////////////////////////
//CHECK#2
// Invoke instance of foo object
var obj__ = new FooObj;

obj__.__proto__.test = "value";

if (protoObj.isPrototypeOf(obj__)) {
    $ERROR('#2.3: protoObj={}; function FooObj(){}; var obj__= new FooObj; protoObj.isPrototypeOf(obj__) === false. Actual: ' + (protoObj.isPrototypeOf(obj__)));
};
// Establish inheritance from proto object
FooObj.prototype = protoObj;

if (protoObj.isPrototypeOf(obj__)) {
    $ERROR('#2.4: protoObj={}; function FooObj(){}; var obj__= new FooObj; FooObj.prototype=protoObj; protoObj.isPrototypeOf(obj__) === false. Actual: ' + (protoObj.isPrototypeOf(obj__)));
};
//
//////////////////////////////////////////////////////////////////////////////

//////////////////////////////////////////////////////////////////////////////
//CHECK#3

// Invoke instance of foo object
var __foo = new FooObj;

if (!Object.prototype.isPrototypeOf(__foo)) {
    $ERROR('#3.1: protoObj={}; function FooObj(){}; var obj__= new FooObj; FooObj.prototype=protoObj; var __foo=new FooObj; Object.prototype.isPrototypeOf(__foo) === true. Actual: ' + (Object.prototype.isPrototypeOf(__foo)));
};

if (!FooObj.prototype.isPrototypeOf(__foo)) {
    $ERROR('#3.2: protoObj={}; function FooObj(){}; var obj__= new FooObj; FooObj.prototype=protoObj; var __foo=new FooObj; FooObj.prototype.isPrototypeOf(__foo) === true. Actual: ' + (FooObj.prototype.isPrototypeOf(__foo)));
};

if (!protoObj.isPrototypeOf(__foo)) {
    $ERROR('#3.3: protoObj={}; function FooObj(){}; var obj__= new FooObj; FooObj.prototype=protoObj; var __foo=new FooObj; protoObj.isPrototypeOf(__foo) === true. Actual: ' + (protoObj.isPrototypeOf(__foo)));
};