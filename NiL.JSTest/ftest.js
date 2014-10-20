var $ERROR = console.log;

console.log(function () {
    var x = [1, 2, 3, 4, 5].sort();
    x.length = 0;
    return x[2];
}());