var $ERROR = console.log;
var print = console.log;

console.log(function () {
    var sum;
    return (sum = function (x, y) {
        if (x <= 0)
            return y;
        if (y === undefined)
            return sum(x - 1, x);
        else
            return sum(x - 1, y + x);
    }), sum(3);
}());