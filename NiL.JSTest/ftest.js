$ERROR = console.log;
$FAIL = console.log;
console.log(function () {
    return /\cA/.exec(String.fromCharCode('A'.charCodeAt(0) % 32));
}());