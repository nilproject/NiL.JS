var $ERROR = console.log;

console.log(function () {
    var a = { 0: "hello", length : 1 };
    return [].shift.call(a);
}());