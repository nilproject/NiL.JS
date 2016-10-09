const dummy = ()=>{};
const print = (x) => {
    if (x !== '')
        console.log(x);
};

if (true) {
    if (false)
        print("'if (false)' fail");
}
else {
    print("'if (true)' fail");
}

var b = true;
if (!b)
    print("if (!true) fail")
b = false;
if (b)
    print("if (false) fail")

b = 1 == 1;
if (!b)
    print("'1 == 1' fail");
b = 1 == 1.0;
if (!b)
    print("'1 == 1.0' fail");
b = 1 == '1';
if (!b)
    print("'1 == '1'' fail");
b = 1 == '1.0';
if (!b)
    print("'1 == '1.0'' fail");
b = 1 == '1.0str';
if (b)
    print("'1 == '1.00str'' fail");
b = 1 != 1;
if (b)
    print("'1 != 1' fail");
b = 1 != 1.0;
if (b)
    print("'1 != 1.0' fail");
b = 1 != '1';
if (b)
    print("'1 != '1'' fail");
b = 1 != '1.0';
if (b)
    print("'1 != '1.0'' fail");
b = 1 != '1.0str';
if (!b)
    print("'1 != '1.00str'' fail");

b = 1.0 == 1;
if (!b)
    print("'1.0 == 1' fail");
b = 1.0 == 1.0;
if (!b)
    print("'1.0 == 1.0' fail");
b = 1.0 == '1';
if (!b)
    print("'1.0 == '1'' fail");
b = 1.0 == '1.0';
if (!b)
    print("'1.0 == '1.0'' fail");
b = 1.0 != 1;
if (b)
    print("'1.0 != 1' fail");
b = 1.0 != 1.0;
if (b)
    print("'1.0 != 1.0' fail");
b = 1.0 != '1';
if (b)
    print("'1.0 != '1'' fail");
b = 1.0 != '1.0';
if (b)
    print("'1.0 != '1.0'' fail");
b = 1.0 != '1.0str';
if (!b)
    print("'1.0 != '1.0str'' fail");

b = 'a' == 'a';
if (!b)
    print("'a' == 'a'' fail");
b = 'a' != 'a';
if (b)
    print("'a' != 'a'' fail");

if (1 != true)
    print("1 != true");
if (0 != false)
    print("0 != false");
if (true != 1)
    print("true != 1");
if (false != 0)
    print("false != 0");
if (!false)
{ }
else
    print("!false fail");

if (0. != 0)
    print("0. != 0");
if (.0 != 0)
    print(".0 != 0");
if (0. != 0.0)
    print("0. != 0.0");
if (.0 != 0.0)
    print(".0 != 0.0");

if (null != null)
    print("null != null fail");
if (null == 0)
    print("null == 0 fail");
if (!(null != null))
    print("");
if (!(null == 0))
    print("");

if (1)
    print("");
else
    print("if(1) fail");
if (0)
    print("if(0) fail");
else
    print("");

if ({})
    print("");
else
    print("if({}) fail");
if (!{})
    print("if(!{}) fail");
else
    print("");
var a = {}
if (a == a)
    dummy();
else
    print('if(a==a) fail');
if (a != a)
    print('if(a!=a) fail');
else
    dummy();
if (a === a)
    dummy();
else
    print('if(a===a) fail');
if (a !== a)
    print('if(a!==a) fail');
else
    dummy();

for (var i = 0; i < 2; i++) if (i) break;
print([, "", "fail"][i]);

_for: for (var i = 0; i < 2; i++) if (i) break _for;
print([, "", "fail"][i]);

for (var i = 1; i > 0; i--) if (i) {
    if (i)
        continue;
    break;
}
print(["", "fail"][i]);

_for: for (var i = 1; i > 0; i--) if (i) {
    if (i)
        continue _for;
    break;
}
print(["", "fail"][i]);

var i = 1;
do {
    if (i)
        continue;
    break;
} while (i--);
print(["", "fail"][i]);

var i = 1;
_do: do {
    if (i)
        continue _do;
    break;
} while (i--);
print(["", "fail"][i]);

_for: for (var j = 0; ; j++) {
    for (var i in [, 1, 2]) {
        if (!j)
            continue _for;
        break;
    }
    break;
}
print(["fail", ""][j]);
function s(x) {
    return function () { return x };
}
var a = s(1);
var b = s(2);
if (a() != 1)
    print("scope #1 fail");
if (b() != 2)
    print("scope #2 fail");
if (s(3)() != 3)
    print("scope #3 fail");
if (a() != 1)
    print("scope #1 fail");
if (b() != 2)
    print("scope #2 fail");
if (s(3)() != 3)
    print("scope #3 fail");

if (!(function () {
    do {

        break;
    } while (false);
    print("");
    return true;
})())
    print("dead do-while fail")

function mul(a, mul) {
    return a * mul;
}
if (mul(2, 2) != 4)
    print("function args link fail")

for (var a = 2; a; (a-- , a--));

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
        print("-1 >>> 0 fail");
})((1 << 30) * 4 - 1, 0);

var a = {};
var b = {};
if (a == (a = b, b))
    print("(a == (a = b, b)) fail");

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

} ());

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

for (a = 1; false;);

for (a == 1; false;);

for (var p in { a: 1, b: 2 });

for (undefinedVariable in {});

(function () {
    if (isNaN(+undefined))
        return
})();

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

function sum(x, r) {
    if (x <= 0)
        return r;
    return sum(x - 1, r + x);
}
sum(10000, 0);

try {
    if (Const != undefined)
        throw "Invalid const predefined value";
    print("TDZ didn't throw exception");
}
catch (e) {

}
const Const = 0;
Const++;
if (Const != 0)
    throw "Const was rewritten";

if (Object);

if (Object); else;

while (!Object);

do; while (!Object);

if ((function (x) {
    var x;
    return x;
})(1) != 1)
    throw "Parameter redefinition fail";

if (0 * 1 + 2 ? false : true)
    throw "Tree of condition of conditional operator has not been rebuilded #1";

if (undefined = 0 * 1 + 2 ? false : true)
    throw "Tree of condition of conditional operator has not been rebuilded #2";

if ((true ? 0 * 1 + 2 : false) != 2)
    throw "Tree of condition of conditional operator has not been rebuilded #3";

if ((true ? undefined = 0 * 1 + 2 : false) != 2)
    throw "Tree of condition of conditional operator has not been rebuilded #4";

if ((false ? false : 0 * 1 + 2) != 2)
    throw "Tree of condition of conditional operator has not been rebuilded #5";

if ((false ? false : undefined = 0 * 1 + 2) != 2)
    throw "Tree of condition of conditional operator has not been rebuilded #6";

print((function (x) {
    switch (x) {
        case 0:
            {
                var a;
                print(x);
                break;
            }
        case 1:
            var b;
            x = "";
            break;
        default
            :
    }
    return x;
})(1));