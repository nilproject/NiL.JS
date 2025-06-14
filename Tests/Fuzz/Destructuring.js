"use strict";

(function test0() {
    var src = [, 2, 3, 4, 5];
    src.pop = function fakePop() { };
    var [s = 999, , x, ...{ pop, ...r }] = src;
    console.assert(s == 999, "assert 0");
    console.assert(pop.name == 'pop', "assert 1");
    console.assert(Array.isArray(r) == false, "assert 2");
    console.assert(Object.keys(r).toString() == '0,1', "assert 3");
    console.assert(Object.keys(r).map(x => r[x]).toString() == '4,5', "assert 4");
})();


(function test1() {
    var src = [, 2, 3, 4, 5];
    var [s = 999, , x, ...[y, ...r]] = src;
    console.assert(s == 999);
    console.assert(y == 4);
    console.assert(r.length == 1);
    console.assert(r[0] == 5);
    console.assert(Array.isArray(r));
})();

(function test2() {
    var src = { a: 1, b: 2, c: 3 };
    var { a: aVal, b, ...r } = src;
    console.assert(typeof a == 'undefined');
    console.assert(aVal == 1);
    console.assert(b == 2);
    console.assert(r.c == 3);
    console.assert(Object.keys(r).length == 1);
})();

(function test3() {
    var src = { a: 1, b: 2, c: 3 };
    var { ['a']: aVal, b, ...r } = src;
    console.assert(typeof a == 'undefined');
    console.assert(aVal == 1);
    console.assert(b == 2);
    console.assert(r.c == 3);
    console.assert(Object.keys(r).length == 1);
})();

(function test4() {
    var lambda = ({ a, b, c }) => [a, b, c];
    var res = lambda({ a: 1, b: 2, c: 3 });
    console.assert(res[0] == 1);
    console.assert(res[1] == 2);
    console.assert(res[2] == 3);
    console.assert(typeof a === 'undefined');
    console.assert(typeof b === 'undefined');
    console.assert(typeof c === 'undefined');
})();

(function test5() {
    var lambda = function ({ a, b, c }) { var fake1 = 1; return [a, b, c]; }
    var res = lambda({ a: 1, b: 2, c: 3 });
    console.assert(res[0] == 1);
    console.assert(res[1] == 2);
    console.assert(res[2] == 3);
    console.assert(typeof a === 'undefined');
    console.assert(typeof b === 'undefined');
    console.assert(typeof c === 'undefined');
})();

(function test6() {
    var lambda = function (arg) { var { a, b, c } = arg; return [a, b, c]; }
    var res = lambda({ a: 1, b: 2, c: 3 });
    console.assert(res[0] == 1);
    console.assert(res[1] == 2);
    console.assert(res[2] == 3);
    console.assert(typeof a === 'undefined');
    console.assert(typeof b === 'undefined');
    console.assert(typeof c === 'undefined');
})();

(function test7() {
    var lambda = function (arg) { const { a, b, c } = arg; return [a, b, c]; }
    var res = lambda({ a: 1, b: 2, c: 3 });
    console.assert(res[0] == 1);
    console.assert(res[1] == 2);
    console.assert(res[2] == 3);
    console.assert(typeof a === 'undefined');
    console.assert(typeof b === 'undefined');
    console.assert(typeof c === 'undefined');
})();

(function test8() {
    var lambda = function (arg) {
        let a = 999, b = 999, c = 999;
        {
            const { a, b, c } = arg;
            return [a, b, c];
        }
    }
    var res = lambda({ a: 1, b: 2, c: 3 });
    console.assert(res[0] == 1);
    console.assert(res[1] == 2);
    console.assert(res[2] == 3);
    console.assert(typeof a === 'undefined');
    console.assert(typeof b === 'undefined');
    console.assert(typeof c === 'undefined');
})();

(function test9() {
    var lambda = ([a, ...rest]) => [a, ...rest];
    var res = lambda([1, 2, 3]);
    console.assert(res[0] == 1);
    console.assert(res[1] == 2);
    console.assert(res[2] == 3);
    console.assert(typeof a === 'undefined');
})();

(function test10() {
    function f({ a } = { a: 1 }, c = f({ a: 1 }, 2)) {
        if (!c)
            return 1;
    }
    f();
})();