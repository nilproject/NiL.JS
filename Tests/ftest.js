var s = Symbol();
console.assert(typeof s === 'symbol', 'typeof#1');
console.assert(typeof Symbol() === 'symbol', 'typeof#2');
try {
    new Symbol();
    console.error("ERROR: Symbol() was created over new")
}
catch (e) {

}

console.assert(Symbol().toString() == "Symbol()", 'Symbol().toString() != "Symbol()"');
console.assert(Symbol("test").toString() == "Symbol(test)", 'Symbol("test").toString() != "Symbol(test)"');
console.assert(!(Symbol() == Symbol()), '!(Symbol() == Symbol())');
console.assert(!(Symbol() === Symbol()), '!(Symbol() === Symbol()))');
console.assert(!(Symbol() <= Symbol()), '!(Symbol() <= Symbol())');
console.assert(!(Symbol() >= Symbol()), '!(Symbol() >= Symbol())');
console.assert(Symbol() != Symbol(), 'Symbol() != Symbol()');
console.assert(Symbol() !== Symbol(), 'Symbol() !== Symbol()');

console.assert(Symbol("symbol") == Symbol.for("symbol"), 'Symbol("symbol") == Symbol.for("symbol")');