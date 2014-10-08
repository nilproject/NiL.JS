$ERROR = console.log;
$FAIL = console.log;
console.log(function () {
    var obj = {};
    obj.slice = Array.prototype.slice;
    obj[0] = 0;
    obj[1] = 1;
    obj[2] = 2;
    obj[3] = 3;
    obj[4] = 4;
    obj.length = 5;
    var arr = obj.slice(0, 3);

    //CHECK#1
    arr.getClass = Object.prototype.toString;
    if (arr.getClass() !== "[object " + "Array" + "]") {
        $ERROR('#1: var obj = {}; obj.slice = Array.prototype.slice; obj[0] = 0; obj[1] = 1; obj[2] = 2; obj[3] = 3; obj[4] = 4; obj.length = 5; var arr = obj.slice(0,3); arr is Array object. Actual: ' + (arr.getClass()));
    }

    //CHECK#2
    if (arr.length !== 3) {
        $ERROR('#2: var obj = {}; obj.slice = Array.prototype.slice; obj[0] = 0; obj[1] = 1; obj[2] = 2; obj[3] = 3; obj[4] = 4; obj.length = 5; var arr = obj.slice(0,3); arr.length === 3. Actual: ' + (arr.length));
    }

    //CHECK#3
    if (arr[0] !== 0) {
        $ERROR('#3: var obj = {}; obj.slice = Array.prototype.slice; obj[0] = 0; obj[1] = 1; obj[2] = 2; obj[3] = 3; obj[4] = 4; obj.length = 5; var arr = obj.slice(0,3); arr[0] === 0. Actual: ' + (arr[0]));
    }
}());