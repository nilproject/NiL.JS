function foo(b) {
    var a = b | 0;
    var x, y;
    x = a;
    y = a >>> 0;
    return [x, y];
}

console.log(foo(-1));