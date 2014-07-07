console.log(function () {
    var result = true;
    var interval = [[0x00, 0x29], [0x40, 0x40], [0x47, 0x60], [0x67, 0xFFFF]];
    for (var indexI = 0; indexI < interval.length; indexI++) {
        for (var indexJ = interval[indexI][0]; indexJ <= interval[indexI][1]; indexJ++) {
            try {
                decodeURI("%C0%" + String.fromCharCode(indexJ, indexJ));
                result = false;
            } catch (e) {
                if ((e instanceof URIError) !== true) {
                    result = false;
                }
            }
        }
    }

    if (result !== true) {
        $ERROR('#1: If B = 110xxxxx (n = 2) and (string.charAt(k + 4) and  string.charAt(k + 5)) do not represent hexadecimal digits, throw URIError');
    }
}());