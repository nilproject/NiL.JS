$ERROR = console.log;
$FAIL = console.log;
console.log(function () {
    switch ('1234'[0])
    {
        case '1':
            return true;
    }
}());