var x = Object.preventExtensions({});
var y = {};
try {
    x.__proto__ = y;
} catch (err) {
    // As far as this test is concerned, we allow the above assignment
    // to fail. This failure does violate the spec and should probably
    // be tested separately.
}
if (Object.getPrototypeOf(x) !== Object.prototype) {
    $ERROR("Prototype of non-extensible object mutated");
}