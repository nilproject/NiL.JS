$ERROR = console.log;
$FAIL = console.log;
console.log(function () {
    var arrObj = [];

    Object.defineProperty(arrObj, 4294967296, {
        value: 100
    });

    return arrObj.hasOwnProperty("4294967296") && arrObj.length === 0 && arrObj[4294967296] === 100;;
}());