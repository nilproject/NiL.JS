// CHECK#13
if (Object(Number.MIN_VALUE).valueOf() !== Number.MIN_VALUE) {
    $ERROR('#13: Object(Number.MIN_VALUE).valueOf() === Number.MIN_VALUE. Actual: ' + (Object(Number.MIN_VALUE).valueOf()));
}

// CHECK#14
if (typeof Object(Number.MIN_VALUE) !== "object") {
    $ERROR('#14: typeof Object(Number.MIN_VALUE) === "object". Actual: ' + (typeof Object(Number.MIN_VALUE)));
}

// CHECK#15
if (Object(Number.MIN_VALUE).constructor.prototype !== Number.prototype) {
    $ERROR('#15: Object(Number.MIN_VALUE).constructor.prototype === Number.prototype. Actual: ' + (Object(Number.MIN_VALUE).constructor.prototype));
}

// CHECK#16
if (Object(Number.MAX_VALUE).valueOf() !== Number.MAX_VALUE) {
    $ERROR('#16: Object(Number.MAX_VALUE).valueOf() === Number.MAX_VALUE. Actual: ' + (Object(Number.MAX_VALUE).valueOf()));
}

// CHECK#17
if (typeof Object(Number.MAX_VALUE) !== "object") {
    $ERROR('#17: typeof Object(Number.MAX_VALUE) === "object". Actual: ' + (typeof Object(Number.MAX_VALUE)));
}

// CHECK#18
if (Object(Number.MAX_VALUE).constructor.prototype !== Number.prototype) {
    $ERROR('#18: Object(Number.MAX_VALUE).constructor.prototype === Number.prototype. Actual: ' + (Object(Number.MAX_VALUE).constructor.prototype));
}

// CHECK#19
if (Object(Number.POSITIVE_INFINITY).valueOf() !== Number.POSITIVE_INFINITY) {
    $ERROR('#19: Object(Number.POSITIVE_INFINITY).valueOf() === Number.POSITIVE_INFINITY. Actual: ' + (Object(Number.POSITIVE_INFINITY).valueOf()));
}

// CHECK#20
if (typeof Object(Number.POSITIVE_INFINITY) !== "object") {
    $ERROR('#20: typeof Object(Number.POSITIVE_INFINITY) === "object". Actual: ' + (typeof Object(Number.POSITIVE_INFINITY)));
}

// CHECK#21
if (Object(Number.POSITIVE_INFINITY).constructor.prototype !== Number.prototype) {
    $ERROR('#21: Object(Number.POSITIVE_INFINITY).constructor.prototype === Number.prototype. Actual: ' + (Object(Number.POSITIVE_INFINITY).constructor.prototype));
}

// CHECK#22
if (Object(Number.NEGATIVE_INFINITY).valueOf() !== Number.NEGATIVE_INFINITY) {
    $ERROR('#22: Object(Number.NEGATIVE_INFINITY).valueOf() === Number.NEGATIVE_INFINITY. Actual: ' + (Object(Number.NEGATIVE_INFINITY).valueOf()));
}

// CHECK#23
if (typeof Object(Number.NEGATIVE_INFINITY) !== "object") {
    $ERROR('#23: typeof Object(Number.NEGATIVE_INFINITY) === "object". Actual: ' + (typeof Object(Number.NEGATIVE_INFINITY)));
}

// CHECK#24
if (Object(Number.NEGATIVE_INFINITY).constructor.prototype !== Number.prototype) {
    $ERROR('#24: Object(Number.NEGATIVE_INFINITY).constructor.prototype === Number.prototype. Actual: ' + (Object(Number.NEGATIVE_INFINITY).constructor.prototype));
}

// CHECK#25
if (isNaN(Object(Number.NaN).valueOf()) !== true) {
    $ERROR('#25: Object(Number.NaN).valueOf() === Not-a-Number. Actual: ' + (Object(Number.NaN).valueOf()));
}

// CHECK#26
if (typeof Object(Number.NaN) !== "object") {
    $ERROR('#26: typeof Object(Number.NaN) === "object". Actual: ' + (typeof Object(Number.NaN)));
}

// CHECK#27
if (Object(Number.NaN).constructor.prototype !== Number.prototype) {
    $ERROR('#27: Object(Number.NaN).constructor.prototype === Number.prototype. Actual: ' + (Object(Number.NaN).constructor.prototype));
}

// CHECK#28
if (Object(1.2345).valueOf() !== 1.2345) {
    $ERROR('#28: Object(1.2345).valueOf() === 1.2345. Actual: ' + (Object(1.2345).valueOf()));
}

// CHECK#29
if (typeof Object(1.2345) !== "object") {
    $ERROR('#29: typeof Object(1.2345) === "object". Actual: ' + (typeof Object(1.2345)));
}

// CHECK#30
if (Object(1.2345).constructor.prototype !== Number.prototype) {
    $ERROR('#30: Object(1.2345).constructor.prototype === Number.prototype. Actual: ' + (Object(1.2345).constructor.prototype));
}

// CHECK#31
if (Object(-1.2345).valueOf() !== -1.2345) {
    $ERROR('#31: Object(-1.2345).valueOf() === -1.2345. Actual: ' + (Object(-1.2345).valueOf()));
}

// CHECK#32
if (typeof Object(-1.2345) !== "object") {
    $ERROR('#32: typeof Object(-1.2345) === "object". Actual: ' + (typeof Object(-1.2345)));
}

// CHECK#33
if (Object(-1.2345).constructor.prototype !== Number.prototype) {
    $ERROR('#33: Object(-1.2345).constructor.prototype === Number.prototype. Actual: ' + (Object(-1.2345).constructor.prototype));
}

