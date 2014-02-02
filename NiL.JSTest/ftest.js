if (typeof (NaN) !== "number") {
    $ERROR('#1: typeof(NaN) === "number". Actual: ' + (typeof (NaN)));
}

// CHECK#2
if (isNaN(NaN) !== true) {
    $ERROR('#2: NaN === Not-a-Number. Actual: ' + (NaN));
}

// CHECK#3
if (isFinite(NaN) !== false) {
    $ERROR('#3: NaN === Not-a-Finite. Actual: ' + (NaN));
}