(function (x) {
    x
})();

function f(char, count) {
    var r = "";
    while (count-- > 0)
        r += char;
    return r + (char < 10 ? f(++char, 3) : "");
}

if (f(1, 3) != "111222333444555666777888999101010")
    throw "Reqursive fail";