try {
    eval('(function(){})(,1);');
    console.log("empty func argument fail #1");
}
catch (e) {
    if (!(e instanceof SyntaxError))
        console.log("empty func argument fail #1.1");
}
try {
    eval('(function(){})(1,,1);');
    console.log("empty func argument fail #2");
}
catch (e) {
    if (!(e instanceof SyntaxError))
        console.log("empty func argument fail #2.1");
}
try {
    eval('(function(){})(1,);');
    console.log("empty func argument fail #3");
}
catch (e) {
    if (!(e instanceof SyntaxError))
        console.log("empty func argument fail #3.1");
}

var f = eval("function functionInsideEval(){}");

for ([, ]; false;);

(function () {
    var s0 = Symbol();
    var s1 = Symbol();
    var o = {};
    o[s0] = 1;
    if (o[s0] !== 1)
        console.log("Can not get value by Symbol");
    if (o[s0] === o[s1])
        console.log("Incorrect keying with symbols");
})();

function func(x) {
    return x + 1;
}
(function (x, y) { if (x == y) console.log("a(1) == a(2)"); })(func(1), func(2));

function x2(x) {
    return 2 * x;
}

(function (a, b) { with ({}) if (a == b) console.log("Weak parameters") })(x2(2), x2(3));

if (Math.max(...[1, 2, 3, 4, 5, 6, 7, 8, 9, 0]) != 9)
    console.log("Spread in proxied method work incorrectly");

function func1(a, b, ...rest) {
    if (JSON.stringify(rest) !== JSON.stringify([2, 3, 4]))
        console.log("Invalid rest parameters container");
    if (JSON.stringify(arguments) !== JSON.stringify({ 0: 0, 1: 1, 2: 2, 3: 3, 4: 4 }))
        console.log("Invalid arguments object inside function with rest parameters");
    return arguments.length;
}

var test = {};
test[Symbol.iterator] = function* () {
    for (var i = 0; i < 5; i++)
        yield i;
}

if (func1(...test) !== 5)
    console.log("Function with rest parameters returns invalid value");

(function (...args) {
    if (JSON.stringify(args) !== JSON.stringify([1, 2, 3, 4, 5, 6, 7, 8, 9, 0, "word"]))
        console.log("Spread operator in array definition work incorrectly");
})(...[1, 2, 3, 4, ...[5, 6, 7, 8], 9, 0], "word");

function funcWhichReturnThis() {
    return this;
}

var b0 = funcWhichReturnThis.bind(undefined);

if (b0() !== function () { return this; } ())
    console.log("Invalid this linking for binded function #1");

var b1 = funcWhichReturnThis.bind(null);

if (b1() !== function () { return this; } ())
    console.log("Invalid this linking for binded function #2");

function bindedConstruct() {
    function f() { return this; };
    return new (f.bind({ fake: 'value' }))();
}

if (bindedConstruct().fake !== undefined)
    console.log("Invalid [[Construct]] of binded function");

var i = 1;
(function (p) { (function (x, y) { if (x != 1) console.log("Incorrect parameters processing #1"); })(p, i = 2) })(i);

i = 1;
(function (p) { (function (x, y) { i = 2; if (x != 1) console.log("Incorrect parameters processing #2"); })(p) })(i);

i = 1;
(function (p) { (function (x, y) { i = 2; if (x != 1) console.log("Incorrect parameters processing #3"); })(p, p) })(i);

(function (p) { (function (x, y) { if (x != true) console.log("Incorrect parameters processing #4"); })(p, i = 2) })(true);

(function (p) { (function (x, y) { p = 2; if (x != true) console.log("Incorrect parameters processing #5"); })(p) })(true);

(function (x) { var a = [x]; a[0] = 2; if (x != 1) console.log("Incorrect clone flag in fast function"); })(1);

(function (x) { var a = [x]; a[0] = 2; if (x != 1) console.log("Incorrect clone flag in regular function"); })(1, 2);

if ((new class extends null { get test() { return "hello" } }).test != "hello")
    console.log("Something wrong with classes");

(function () {
    var e = eval;
    e("var mustBeDeclaredInGlobalContext = " + eval("'2'"));

    (1, eval)("var mustBeDeclaredInGlobalContext_second = 2");
})();

if (typeof mustBeDeclaredInGlobalContext === "undefined")
    console.log("Incorrect processing of eval function");
if (typeof mustBeDeclaredInGlobalContext_second === "undefined")
    console.log("Incorrect processing of eval function");

var functionInExpression = function () {
    function test() {
        return 1;
    }

    return test();
}

functionInExpression();

function runEvalTest(x) {
    x();
}

eval('\
function testEval() {\
}\
\
runEvalTest(testEval);');

function twoParametersWithSameName(x, x) {
    return x;
}
if (!(twoParametersWithSameName(1, 2) === 2)) {
    $ERROR("#1: twoParametersWithSameName(1, 2) === 2");
}

class A {
    constructor() {
        this.text = new.target.name;
    }
}

class B extends A { constructor() { super(); } }

var a = new A().text; // logs "A"
var b = new B().text; // logs "B"

if (a != "A")
    console.log("new.target works incorrectly");
if (b != "B")
    console.log("new.target works incorrectly");

a = console ? 1 : 2, { test: 1 };

(function () {
    // parse only
    var r;
    if (r) for (t = 0; u > t; t++) n[t][e][1] /= r; else for (t = 0; u > t; t++) n[t][e][1] = o;
    (this||/[/]/);
});

var undefined;

try {
	undefined.test = 1;
	console.log("'undefined.test = 1' did not thrown exception")
}
catch (e) {
}

try {
	delete undefined.test;
	console.log("'delete undefined.test' did not thrown exception")
}
catch (e) {
}

JSON.stringify(new Date());

if (JSON.stringify([,1])!=='[null,1]')
    console.log(`JSON.stringify works incorrectly (${JSON.stringify([,1])})`);

if (JSON.stringify([,,1])!=='[null,null,1]')
    console.log(`JSON.stringify works incorrectly (${JSON.stringify([,,1])})`);

({get [1](){return 1;},[2]:2})

console.assert(((x) => { var r = "" + x; return r })("123"), "123");

if (Object.name !== "Object")
    throw "Invalid name of Object(...)";

if (Object.toString() !== "function Object() { [native code] }")
    throw "Incorrect toString of Object(...)";

Object.toString = () =>'hello';

if (Object.toString() === "function Object() { [native code] }")
    throw "toString of Object(...) does not change";

if ([1, , 2, 3][['length']] != 4)
    throw "Invalid array.length optimization";

if (isNaN.__proto__ != Function.__proto__)
    throw "Incorrect prototype of ExternalFunction";

[...new Date()];

console.asserta(() => Error.constructor == Function.constructor);
console.asserta(() => Error.constructor().__proto__ == Function.prototype);
console.asserta(() => Object.call(Error).__proto__ == Object.prototype);

if ((function(a = 5) { return a })() != 5)
    throw new 'Something wrong with default parameter value';

console.asserta(() => (new class {}).toString(), {}.toString());
