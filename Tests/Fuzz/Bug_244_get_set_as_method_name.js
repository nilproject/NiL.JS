class AClass {
    set(aValue) {
        this.aClassValue = aValue;
    }

    get() {
        return this.aClassValue;
    }
}

var a = new AClass;
a.set(123);

if (a.get() != 123)
    console.error("a.get() != 123");