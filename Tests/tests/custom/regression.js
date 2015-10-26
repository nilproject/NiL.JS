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

var f = eval("function func(){}");

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

(function (a, b) { with({}) if(a == b) console.log("Weak parameters") })(x2(2), x2(3));