(function () {
    var tokenCodes = {
        null: 0,
        true: 1,
        false: 2
    };
    var arr = [
        'null',
        'true',
        'false'
    ];
    for (var p in tokenCodes) {
        console.log(p);
        for (var p1 in arr) {
            console.log(p1);
            if (arr[p1] === p) {
                if (!tokenCodes.hasOwnProperty(arr[p1])) {
                    return false;
                };
            }
        }
    }
    return true;
}());