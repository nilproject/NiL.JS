var $ERROR = console.log;
var print = console.log;

console.log(function () {
    Array.prototype[0] = 1;
    var x = [, ];
    return x.unshift(2);
}());