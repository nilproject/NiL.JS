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