$ERROR = console.log;
$FAIL = console.log;
console.log(function () {
    var s = new String("abc");
    var desc = Object.getOwnPropertyDescriptor(s, "length");

    if (desc.writable === false &&
        desc.enumerable === false &&
        desc.configurable === false &&
        desc.hasOwnProperty('get') === false &&
        desc.hasOwnProperty('set') === false) {
        return true;
    }
}());