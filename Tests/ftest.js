function func() {
    return arguments.length;
}

var test = {};
test[Symbol.iterator] = function*(){
    yield 1;
    yield 2;
    yield 3;
}

console.log(func(...test));