function sum(a, b) {
    return a + b;
}

(function (b) { console.log(sum(2, 3), b); }(sum(1, 1)));