class Test {
    async test() {
        return 1;
    }
}

var async = { async : true};

console.log(new Test().test())
console.log((async () => 1)());
console.log({ async ['t']() { return 1; } }.t());