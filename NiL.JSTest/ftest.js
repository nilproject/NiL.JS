$ERROR = console.log;
$FAIL = console.log;
console.log(function () {
    var a = 0;
    function cicle(x) {
        if (x == 0)
            return;
        a++;
        cicle(x - 1);
    }
    cicle(10000000);
    return a;
}());