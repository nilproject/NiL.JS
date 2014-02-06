var x = 1;
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
    get x(){
        return this.x;
    }
};
o.f();

if ((1,o.getx)() != 1)
    console.log("#1.1");

function (a) {
    if (a != this)
        console.log("#2.");
} (this)

if (function () { return this } () != this)
    console.log("#3.");

var o1 = { x: 4, set p(v){this.x = v;} };
var o2 = { y: 3, set p(v){this.y = v;} };
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
console.log("#8");

var o = { x: 1, getx: function () { return function () { return this.x }; } };
var x = 2;
if (o.getx().call(null) != 2)
console.log("#9");