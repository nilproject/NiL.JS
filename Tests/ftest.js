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

function testRest(a, b, ...rest)
{
    console.log(rest);
}
testRest(1,2,3,4,5,6,7);

class BaseClass
{
    constructor()
    {
        this.a = "a";
    }
    get A()
    {
        return this.a;
    }
    getA()
    {
        return this.a;
    }
    static getA()
    {
        return "static a";
    }
    static get A()
    {
        return "static a";
    }
}

class DerivedClass extends BaseClass
{
    constructor()
    {
        super();
        this.b = "b";
    }
    get B()
    {
        return this.b;
    }
}

var a = new DerivedClass();