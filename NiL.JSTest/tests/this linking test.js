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
    }
};
o.f();

function (a) {
    if (a != this)
        console.log("#2.");
} (this)

if (function () { return this } () != this)
    console.log("#3.");