﻿console.log(function () {
    var __arr = [4, 3, 2, 1, 4, 3, 2, 1, 4, 3, 2, 1];
    //Sort uses closure
    //
    __arr.sort(
        function (x, y) {
            if (x > y) { return -1; }
            if (x < y) { return 1; }
            if (x == y) { return 0; }
        }
    );

    //////////////////////////////////////////////////////////////////////////////
    //CHECK#1
    if (__arr.toString() !== [4, 4, 4, 3, 3, 3, 2, 2, 2, 1, 1, 1].toString()) {
        $ERROR('#1: __arr.toString() === [4,4,4,3,3,3,2,2,2,1,1,1].toString(). Actual: __arr.toString() ===' + __arr.toString());
    }
}());