$ERROR = console.log;
$FAIL = console.log;
console.log(function () {
    var a = 1;
    return -0x80000000 - a;
}());