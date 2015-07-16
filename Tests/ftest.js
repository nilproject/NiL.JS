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