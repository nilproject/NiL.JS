var obj = null;
var a = 0;
var b = 0;
var c = 0;

function* foo() {
    obj = {
        a: a = 1,
        b: b = yield 2,
        c: c = 'c',
    };
}

var fooIter = foo();

var val = fooIter.next();
Debug.asserta(() => val.value == 2 && !val.done);
Debug.asserta(() => obj == null);
Debug.asserta(() => a == 1);
Debug.asserta(() => b == 0);
Debug.asserta(() => c == 0);

val = fooIter.next('b');
Debug.asserta(() => val.value == undefined && val.done);
Debug.asserta(() => obj != null);
Debug.asserta(() => obj.b == 'b');
Debug.asserta(() => a == 1);
Debug.asserta(() => b == 'b');
Debug.asserta(() => c == 'c');