if (String(NaN) !== "NaN") {
    $ERROR('#1: String(NaN) === Not-a-Number Actual: ' + (String(NaN)));
}

// CHECK#2
if (String(Number.NaN) !== "NaN") {
    $ERROR('#2: String(Number.NaN) === Not-a-Number Actual: ' + (String(Number.NaN)));
}

// CHECK#3
if (String(Number("asasa")) !== "NaN") {
    $ERROR('#3: String(Number("asasa")) === Not-a-Number Actual: ' + (String(Number("asasa"))));
}