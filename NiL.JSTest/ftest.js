var x = 0;

var myObj = { x: "obj" };

function f1() {
    var x = 1;
    function f2() {
        with (myObj) {
            return x;
        }
    };
    return f2();
}

console.log(f1())