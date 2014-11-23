

function f(char, count) {
    console.log(char, count);
    var r = "";
    while (count-- > 0)
        r += char;
    return r + (char < 3 ? f(++char, 3) : "");
}

console.log(f(1, 3));

/////////////////////////////////////////////////////////////////////
function shouldBe(x, y) {
    var rx = eval(x);
    var ry = eval(y);
    if (rx !== ry) {
        //console.log(x + " !== " + y);
        //console.log(rx);
        //console.log();
        //console.log(ry);
    }
}

function shouldBeTrue(x) {
    return shouldBe(x, true);
}

function shouldBeFalse(x) {
    return shouldBe(x, false);
}

function shouldThrow(x) {
    try {
        eval(x);
        return false;
    }
    catch (e) {
        return true;
    }
}