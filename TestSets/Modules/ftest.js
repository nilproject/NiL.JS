(function () {

    console.log({ a: 2, b: 'abc', c: Math, d: Object, e: [1, 2] });

    return;

    console.log({ a: 1, ["a"]: 2 });
    console.log({ ["a"]: 2, a: 1 });

    return;

    //var oldObject = { a: 1, b: 2 };
    //var newObject = { ...oldObject, c: 3 };
    //console.log(newObject);

    return;

    function parseCssTime(time) {
        var point = false;
        var pointCharCode = 46;
        var zeroCharCode = 48;
        var nineCharCode = 57;
        var mCharCode = 109;
        var sCharCode = 115;
        var mul = 1;
        for (var i = 0; i < time.length; i++) {
            var charCode = time.charCodeAt(i);
            if (charCode == pointCharCode) {
                if (point)
                    return NaN;
                point = true;
            } else if (charCode == mCharCode) {
                if (time.charCodeAt(i + 1) != sCharCode)
                    return NaN;
                i++;
            } else if (charCode == sCharCode) {
                if (time.length > i + 1)
                    return NaN;
                mul = 1000;
            } else if (
                charCode != mCharCode
                && charCode != sCharCode
                && charCode != mCharCode
                && (charCode < zeroCharCode || charCode > nineCharCode)) {
                return NaN;
            }
        }

        return parseFloat(time) * mul;
    }

    console.log(parseCssTime('0.2s'));
    console.log(parseCssTime('0.2ms'));
    console.log(parseCssTime('2s'));
    console.log(parseCssTime('2ms'));
    console.log(parseCssTime('9.ms'));
    console.log(parseCssTime('.9ms'));
    console.log(parseCssTime('9.9.ms'));

    return;

    console.log(3 ** 3);

    return;

    console.log(10.67e+40.toPrecision(100));
    return;

    var a = /a//1
    var b = 1/1/a//;
    var s = "//";
    var q = /\/**/

    var __re = new RegExp;

    console.log(Object.getOwnPropertyNames(__re));
    console.log(delete __re.lastIndex);
    console.log(Object.getOwnPropertyNames(__re));
    console.log(__re.lastIndex);

    return;

    //console.log(0.1);
    //console.log(5.0000000000000093)
    //console.log(5.0000000000000094)
    //console.log(5.00000000000001);
    //console.log(1.5);
    //console.log(1.25);
    //console.log(1.1);
    //console.log(0.00123439999999999992);

    //1.2
    //3.5
    //3.5e-1
    //1e+1
    //10000000000000000000.0000035e+37
    //123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789
    //123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789.123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789e+60
    //0
    //100e+100
    //100.01
    //1.2345
    //100.01e+100
    //0.123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789e+60
    //1.234567891234568e+107
    //0.0012344999999999999
    //-1
    //.0
    //0.
    //console.log(1e-1);
    //console.log(0.1);
    //console.log(1e-1 === 0.1);
    //4.9406564584124654417656879286822e-324;
    //1.797693134862315807e+308
    //1.797693134862315808e+308
    //10e10000
    //4.9407e-324
    //5e-324

    //return;

    //console.log(parseFloat("0.0012344999999999998"));

    //console.log(8.06);
    //console.log(10.36);
    //console.log(9.12);
    //console.log(1.9999999999999999);

    return;

    console.log(10.669999999999993);

    console.log(10.66999999999999);
    console.log(10.67);

    console.log(10.42999999999999);
    console.log(10.43);

    console.log(20.98999999999999);
    console.log(20.99);

    return;

    console.log(100111122133144155);
    console.log(6776767677.006771122677555);

    console.log(0.0012344999999999997);
    console.log(0.0012344999999999998);
    console.log(0.0012344999999999999);
    console.log(0.0012345);
    console.log(0.12345);
    console.log(1.2345);
    console.log(69.84999999999992);
    console.log(69.84999999999993);
    console.log(69.84999999999994);
    console.log(69.84999999999995);
    console.log(69.84999999999996);
    console.log(69.84999999999997);
    console.log(69.84999999999998);
    console.log(69.84999999999999);
    console.log(69.85000000000000);
    console.log(69.85000000000001);
    console.log(100e+100);
    console.log(100.01);
    console.log(0.0625);
    console.log(parseFloat('69.85'));
    console.log(5.0000000000000093)
    console.log(5.0000000000000094)
    console.log(5.00000000000001);
})();