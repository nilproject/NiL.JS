var $ERROR = console.log;
var print = console.log;

console.log(function () {
    var a = ["zero", , "two"];
    Object.defineProperty(a, "1", {
        get: function () { return "one"; },
        set: function (v) { console.log(v); },
        enumerable: true,
        configurable: true
    });
    console.log(JSON.stringify(a));
    console.log([].shift.call(a));
    return JSON.stringify(a);

    function shouldBeTrue(x) {
        if (eval(x) !== true)
            print("FAIL(true): " + x);
    }
    function shouldBeFalse(x) {
        if (eval(x) !== false)
            print("FAIL(false): " + x);
    }

    function test(f) {

        var testObj = {
            length: 3
        };
        var propertyGetter = {
            get: (function () { throw true; })
        }
        Object.defineProperty(testObj, 0, propertyGetter);
        Object.defineProperty(testObj, 1, propertyGetter);
        Object.defineProperty(testObj, 2, propertyGetter);

        try {
            f.call(testObj, function () { });
            return false;
        } catch (e) {
            return e === true;
        }
    }

    // This test makes sense for these functions: (they should get all properties on the array)
    shouldBeTrue("test(Array.prototype.sort)");
    shouldBeTrue("test(Array.prototype.every)");
    shouldBeTrue("test(Array.prototype.some)");
    shouldBeTrue("test(Array.prototype.forEach)");
    shouldBeTrue("test(Array.prototype.map)");
    shouldBeTrue("test(Array.prototype.filter)");
    shouldBeTrue("test(Array.prototype.reduce)");
    shouldBeTrue("test(Array.prototype.reduceRight)");

    // Probably not testing much of anything in these cases, but make sure they don't crash!
    shouldBeTrue("test(Array.prototype.join)");
    shouldBeTrue("test(Array.prototype.pop)");
    shouldBeFalse("test(Array.prototype.push)");
    shouldBeTrue("test(Array.prototype.reverse)");
    shouldBeTrue("test(Array.prototype.shift)");
    shouldBeTrue("test(Array.prototype.slice)");
    shouldBeTrue("test(Array.prototype.splice)");
    shouldBeTrue("test(Array.prototype.unshift)");
    shouldBeTrue("test(Array.prototype.indexOf)");
    shouldBeTrue("test(Array.prototype.lastIndexOf)");
}());