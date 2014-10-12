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