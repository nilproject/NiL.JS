$ERROR = console.log;
$FAIL = console.log;
console.log(function () {    
    function* gen() {
        console.log(yield "hello");
        return yield "world";
    }
    var g = gen();
    console.log(g.next().value);
    console.log(g.next("middle").value);
    console.log(g.next("finall").value);
} ());