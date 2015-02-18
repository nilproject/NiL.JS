if (true) {
    if (false)
        console.log("'if (false)' fail");
}
else {
    console.log("'if (true)' fail");
}

var b = true;
if (!b)
    console.log("if (!true) fail")
b = false;
if (b)
    console.log("if (false) fail")

b = 1 == 1;
if (!b)
    console.log("'1 == 1' fail");
b = 1 == 1.0;
if (!b)
    console.log("'1 == 1.0' fail");
b = 1 == '1';
if (!b)
    console.log("'1 == '1'' fail");
b = 1 == '1.0';
if (!b)
    console.log("'1 == '1.0'' fail");
b = 1 == '1.0str';
if (b)
    console.log("'1 == '1.00str'' fail");
b = 1 != 1;
if (b)
    console.log("'1 != 1' fail");
b = 1 != 1.0;
if (b)
    console.log("'1 != 1.0' fail");
b = 1 != '1';
if (b)
    console.log("'1 != '1'' fail");
b = 1 != '1.0';
if (b)
    console.log("'1 != '1.0'' fail");
b = 1 != '1.0str';
if (!b)
    console.log("'1 != '1.00str'' fail");

b = 1.0 == 1;
if (!b)
    console.log("'1.0 == 1' fail");
b = 1.0 == 1.0;
if (!b)
    console.log("'1.0 == 1.0' fail");
b = 1.0 == '1';
if (!b)
    console.log("'1.0 == '1'' fail");
b = 1.0 == '1.0';
if (!b)
    console.log("'1.0 == '1.0'' fail");
b = 1.0 != 1;
if (b)
    console.log("'1.0 != 1' fail");
b = 1.0 != 1.0;
if (b)
    console.log("'1.0 != 1.0' fail");
b = 1.0 != '1';
if (b)
    console.log("'1.0 != '1'' fail");
b = 1.0 != '1.0';
if (b)
    console.log("'1.0 != '1.0'' fail");
b = 1.0 != '1.0str';
if (!b)
    console.log("'1.0 != '1.0str'' fail");

b = 'a' == 'a';
if (!b)
    console.log("'a' == 'a'' fail");
b = 'a' != 'a';
if (b)
    console.log("'a' != 'a'' fail");

if (1 != true)
    console.log("1 != true");
if (0 != false)
    console.log("0 != false");
if (true != 1)
    console.log("true != 1");
if (false != 0)
    console.log("false != 0");
if (!false)
{ }
else
    console.log("!false fail");

if (0. != 0)
    console.log("0. != 0");
if (.0 != 0)
    console.log(".0 != 0");
if (0. != 0.0)
    console.log("0. != 0.0");
if (.0 != 0.0)
    console.log(".0 != 0.0");

if (null != null)
    console.log("null != null fail");
if (null == 0)
    console.log("null == 0 fail");
if (!(null != null))
    console.log("!(null != null) pass");
if (!(null == 0))
    console.log("!(null == 0) pass");

if (1)
    console.log("if(1) pass");
else
    console.log("if(1) fail");
if (0)
    console.log("if(0) fail");
else
    console.log("if(0) pass");

if ({})
    console.log("if({}) pass");
else
    console.log("if({}) fail");
if (!{})
    console.log("if(!{}) fail");
else
    console.log("if(!{}) pass");
var a = {}
if (a == a)
    console.log('if(a==a) pass');
else
    console.log('if(a==a) fail');
if (a != a)
    console.log('if(a!=a) fail');
else
    console.log('if(a!=a) pass');
if (a === a)
    console.log('if(a===a) pass');
else
    console.log('if(a===a) fail');
if (a !== a)
    console.log('if(a!==a) fail');
else
    console.log('if(a!==a) pass');

for (var i = 0; i < 2; i++) if (i) break;
console.log([, "pass", "fail"][i]);

_for: for (var i = 0; i < 2; i++) if (i) break _for;
console.log([, "pass", "fail"][i]);

for (var i = 1; i > 0; i--) if (i) {
    if (i)
        continue;
    break;
}
console.log(["pass", "fail"][i]);

_for: for (var i = 1; i > 0; i--) if (i) {
    if (i)
        continue _for;
    break;
}
console.log(["pass", "fail"][i]);

var i = 1;
do {
    if (i)
        continue;
    break;
} while (i--);
console.log(["pass", "fail"][i]);

var i = 1;
_do: do {
    if (i)
        continue _do;
    break;
} while (i--);
console.log(["pass", "fail"][i]);

_for: for (var j = 0; ; j++) {
    for (var i in [, 1, 2]) {
        if (!j)
            continue _for;
        break;
    }
    break;
}
console.log(["fail", "pass"][j]);
function s(x) {
    return function () { return x };
}
var a = s(1);
var b = s(2);
if (a() != 1)
    console.log("scope #1 fail");
if (b() != 2)
    console.log("scope #2 fail");
if (s(3)() != 3)
    console.log("scope #3 fail");
if (a() != 1)
    console.log("scope #1 fail");
if (b() != 2)
    console.log("scope #2 fail");
if (s(3)() != 3)
    console.log("scope #3 fail");

if (!(function () {
    do {

        break;
} while (false);
    console.log("dead do-while pass");
    return true;
})())
    console.log("dead do-while fail")

function mul(a, mul) {
    return a * mul;
}
if (mul(2, 2) != 4)
    console.log("function args link fail")

for (var a = 2; a; (a--, a--));

(function () {
    function selfReferencedFunction() {
        return selfReferencedFunction.prototype;
    }
    return selfReferencedFunction();
})();

function test(x) {
    (function () {
        var a = 1;
        if (x > 0)
            test(x - 1);
        return (x, a);
    })();
}
test(1);

(function (x, zero) {
    if ((x != -1 >>> 0)
        || (x != -1 >>> zero))
        console.log("-1 >>> 0 fail");
})((1 << 30) * 4 - 1, 0);

var a = {};
var b = {};
if (a == (a = b, b))
    console.log("(a == (a = b, b)) fail");

1[0]

function f(char, count) {
    var r = "";
    while (count-- > 0)
        r += char;
    return r + (char < 10 ? f(++char, 3) : "");
}

if (f(1, 3) != "111222333444555666777888999101010")
    throw "Reqursive fail";

a[0] = b[1] = "";

a = 1;
(function (x) { if (x != 1) throw "Parameter not saved"; })(a, a = 2);

var __func = function __exp__func(arg) {
    if (arg === 1) {
        return arg;
    } else {
        return __exp__func(arg - 1) * arg;
    }
};

if (__func(3) != 6)
    throw "fact";

-0x1;

(function () {
    var a = 1;

    var o = { a: 2 };
    try {
        with (o) {
            a = 3;
            throw 1;
            a = 4;
        }
    }
    catch (e)
    { }

    if (a === 1 && o.a === 3) {
        return true;
    }

}());

(function () {
    a;
    a = 1;
    for (var a = false in {});
    if (a !== false)
        throw "for..in with default value";
})();

(function () {
    a;
    a = 1;
    for (var a = false of {});
    if (a !== false)
        throw "for..of with default value";
})();

for ( a = 1 ; false;);

for ( a == 1 ; false;);

for (var p in { a: 1, b: 2 });

for(undefinedVariable in {});

(function(){
    if (isNaN(+undefined))
        return})();

(function () {
    function macro() {
        return 10;
    }
    (function () { macro(); })()
})();

function RegExpf() {
    return /./;
}

if (RegExpf() === RegExpf())
    throw "Need to recreate regex";

if (Function.prototype.prototype !== undefined)
    throw "Function.prototype.prototype mast be undefined";

var jsonprs = JSON.parse('{ "True" : true, "False" : false, "Null": null, "One": 1, "Pi": 3.14159265358, "String": "This Is String", "Negative one": -1, "Negative Pi": -3.14159265358 }');

if (jsonprs.False !== false)
    throw "false parsed with error"

if (jsonprs.True !== true)
    throw "true parsed with error"

if (jsonprs.Null !== null)
    throw "null parsed with error"

if (jsonprs.One !== 1)
    throw "one parsed with error"

if (jsonprs.Pi !== 3.14159265358)
    throw "pi parsed with error"

if (jsonprs.String !== "This Is String")
    throw "string parsed with error"

if (jsonprs["Negative one"] !== -1)
    throw "Negative one parsed with error"

if (jsonprs["Negative Pi"] !== -3.14159265358)
    throw "Negative Pi parsed with error"