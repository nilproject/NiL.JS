var $ERROR = console.log;
var print = console.log;

console.log(function () {
    var x = [];
    Array.prototype[0] = 1;
    x.length = 1;
    return x.unshift(0);
}());