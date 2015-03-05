console.log(function () {

    var $ERROR = console.log;

    //CHECK#5
    var x = [];
    x[0] = 0;
    x[3] = 3;
    var shift = x.shift();
    if (shift !== 0) {
        $ERROR('#5: x = []; x[0] = 0; x[3] = 3; x.shift() === 0. Actual: ' + (shift));
    }

    //CHECK#6
    if (x.length !== 3) {
        $ERROR('#6: x = []; x[0] = 0; x[3] = 3; x.shift(); x.length == 3');
    }

    //CHECK#7
    if (x[0] !== undefined) {
        $ERROR('#7: x = []; x[0] = 0; x[3] = 3; x.shift(); x[0] == undefined');
    }

    //CHECK#8
    if (x[12] !== undefined) {
        $ERROR('#8: x = []; x[0] = 0; x[3] = 3; x.shift(); x[1] == undefined');
    }

    //CHECK#9
    x.length = 1;
    var shift = x.shift();
    if (shift !== undefined) {
        $ERROR('#9: x = []; x[0] = 0; x[3] = 3; x.shift(); x.length = 1; x.shift() === undefined. Actual: ' + (shift));
    }

    //CHECK#10
    if (x.length !== 0) {
        $ERROR('#10: x = []; x[0] = 0; x[3] = 3; x.shift(); x.length = 1; x.shift(); x.length === 0. Actual: ' + (x.length));
    }
}());