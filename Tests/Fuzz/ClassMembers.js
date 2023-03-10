(function () {

    function f() { };
    var c = 1;
    class Test {
        d = 1;
        m = 1;
        static s = 2;
        constructor() {
            this.m = c++;
        }
    }

    var t0 = new Test();
    var t1 = new Test();

    console.assert(Test.s == 2, "Error #" + 1);
    console.assert(t0.s === undefined, "Error #" + 2);
    console.assert(t1.s === undefined, "Error #" + 3);
    console.assert(t0.s === undefined, "Error #" + 4);
    console.assert(t0.__proto__.s === undefined, "Error #" + 5)

    console.assert(Test.d === undefined, "Error #" + 6);
    console.assert(t0.d === 1, "Error #" + 7);
    console.assert(t1.d === 1, "Error #" + 8);
    console.assert(t0.d === 1, "Error #" + 9);
    console.assert(t0.__proto__.d === undefined, "Error #" + 10);

    console.assert(Test.m === undefined, "Error #" + 11);
    console.assert(t0.m === 1, "Error #" + 12);
    console.assert(t1.m === 2, "Error #" + 13);
    console.assert(t0.m === 1, "Error #" + 14);
    console.assert(t0.__proto__.m === undefined, "Error #" + 15);
})();