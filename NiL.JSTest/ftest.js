var x = new Array();
var unshift = x.unshift(1);
if (unshift !== 1) {
    $ERROR('#1: x = new Array(); x.unshift(1) === 1. Actual: ' + (unshift));
}

//CHECK#2
if (x[0] !== 1) {
    $ERROR('#2: x = new Array(); x.unshift(1); x[0] === 1. Actual: ' + (x[0]));
}

//CHECK#3
var unshift = x.unshift();
if (unshift !== 1) {
    $ERROR('#3: x = new Array(); x.unshift(1); x.unshift() === 1. Actual: ' + (unshift));
}

//CHECK#4
if (x[1] !== undefined) {
    $ERROR('#4: x = new Array(); x.unshift(1); x.unshift(); x[1] === unedfined. Actual: ' + (x[1]));
}

//CHECK#5
var unshift = x.unshift(-1);
if (unshift !== 2) {
    $ERROR('#5: x = new Array(); x.unshift(1); x.unshift(); x.unshift(-1) === 2. Actual: ' + (unshift));
}

//CHECK#6
if (x[0] !== -1) {
    $ERROR('#6: x = new Array(); x.unshift(1); x.unshift(-1); x[0] === -1. Actual: ' + (x[0]));
}

//CHECK#7
if (x[1] !== 1) {
    $ERROR('#7: x = new Array(); x.unshift(1); x.unshift(-1); x[1] === 1. Actual: ' + (x[1]));
}

//CHECK#8
if (x.length !== 2) {
    $ERROR('#8: x = new Array(); x.unshift(1); x.unshift(); x.unshift(-1); x.length === 2. Actual: ' + (x.length));
}