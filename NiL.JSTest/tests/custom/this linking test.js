if (!this.$ERROR)
    this.$ERROR = console.log;

var x = 1;
function f0() {
    if (this.x != 1) {
        $ERROR("#0. this linking error.");
    }
}
f0();
function f1() {
    if (this.x != 1) {
        $ERROR("#1. this linking error.");
    }
}

var o = {
    x: 2,
    f: function () {
        f1();
    },
    getx: function () {
        return this.x;
    }
};
o.f();

if (o.getx() != 2)
    $ERROR("#1.1");

if ((1, o.getx)() != 1)
    $ERROR("#1.2");

(function (a) {
    if (a != this)
        $ERROR("#2.");
})(this)

if (function () { return this }() != this)
    $ERROR("#3.");

var o1 = { x: 4, set p(v) { this.x = v; } };
var o2 = { y: 3, set p(v) { this.y = v; } };
o2.p = (o1.x / 2);
if (o1.y)
    $ERROR("#4");
if (o1.x != 4)
    $ERROR("#5");
if (o2.y != 2)
    $ERROR("#6");
if (o2.x)
    $ERROR("#7");

String(Math.PI)

var c = function () { }
c.prototype.m1 = function () { return this.m2(); };
c.prototype.m2 = function () { return this.x; };
var t = function () {
    var o = new c();
    o.x = 1;
    return o.m1()
}();
if (t != 1)
    $ERROR("#8");

var o = { x: 0, getx: function () { this.x = 1; return function () { return this.x }; } };
var x = 2;
if (o.getx().call(null) != 2)
    $ERROR("#9");

var x = 1;
var o = {
    x: 2,
    s: {
        '2': function () { return 'fail'; },
        '1': function () { return 'pass'; }
    }
};
var t = o.s[this.x]();
if (t == 'fail')
    $ERROR('#10');

var x = 1;
function f11() {
    this.x = 2;
}
function f12() {
    return this.x;
}
if (f12(new f11()) != 1)
    $ERROR('#11');

var t = { x: 2, f: function () { return function () { return this.x; } } };
if (t.f()() != 1)
    $ERROR("#12");

function func(x) {
    return x + 1;
}
(function (x, y) { if (x == y) console.log("a(1) == a(2)"); })(func(1), func(2));

if ((new (function () { return function () { this.str = 'hello' } }())).str != "hello")
    console.log("new (f()) failed.");