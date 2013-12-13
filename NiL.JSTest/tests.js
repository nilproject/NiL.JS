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
    console.log('if(a==a) pass');
if (a === a)
    console.log('if(a===a) pass');
else
    console.log('if(a===a) fail');
if (a !== a)
    console.log('if(a!==a) fail');
else
    console.log('if(a!==a) pass');