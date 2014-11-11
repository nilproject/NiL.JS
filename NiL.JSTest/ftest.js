function test() {
    var s;
    (s = Number()).p = 1;
    return s.p;
}

console.log(test());