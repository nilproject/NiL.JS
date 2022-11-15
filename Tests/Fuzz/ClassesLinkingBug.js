(function () {
    class NestedClass {
        get someValue() {
            return 1;
        }
    }

    var { someValue } = new NestedClass;

    if (someValue != 1)
        console.error("Fail");
})