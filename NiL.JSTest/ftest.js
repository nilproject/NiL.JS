$ERROR = console.log;
$FAIL = console.log;
console.log(function () {
    return "|" + "    ab\
    \0c    ".trim() + "|";
}());