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
        console.log("Incorrect rest parameters container");
    if (JSON.stringify(arguments) !== JSON.stringify({ 0: 0, 1: 1, 2: 2, 3: 3, 4: 4 }))
        console.log("Incorrect arguments object inside function with rest parameters");
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

var a = 1;
(function (p) { (function (x, y) { if (x != 1) console.log("Incorrect parameters processing #1"); })(p, a = 2) })(a);

a = 1;
(function (p) { (function (x, y) { a = 2; if (x != 1) console.log("Incorrect parameters processing #2"); })(p) })(a);

a = 1;
(function (p) { (function (x, y) { a = 2; if (x != 1) console.log("Incorrect parameters processing #3"); })(p, p) })(a);

(function (p) { (function (x, y) { if (x != true) console.log("Incorrect parameters processing #4"); })(p, a = 2) })(true);

(function (p) { (function (x, y) { p = 2; if (x != true) console.log("Incorrect parameters processing #5"); })(p) })(true);

(function (x) { var a = [x]; a[0] = 2; if (x != 1) console.log("Incorrect clone flag in fast function"); })(1);

(function (x) { var a = [x]; a[0] = 2; if (x != 1) console.log("Incorrect clone flag in regular function"); })(1, 2);

(function (x) { var a = [x]; a[0] = 2; if (x != 1) console.log("Incorrect clone flag in fast function"); })(1);

(function (x) { var a = [x]; a[0] = 2; if (x != 1) console.log("Incorrect clone flag in regular function"); })(1, 2);