var x = 1;
function f1() {
    if (this.x != 1) {
        $ERROR("this linking error.");
    }
}

var o = {
    x: 2,
    f: function () {
        f1();
    }
};
o.f();