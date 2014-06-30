console.log((function () {
    var arrObj = [];

    Object.defineProperty(arrObj, "0", { value: -0 });

    try {
        Object.defineProperty(arrObj, "0", { value: +0 });
        return false;
    } catch (e) {
        return e instanceof TypeError && dataPropertyAttributesAreCorrect(arrObj, "0", -0, false, false, false);
    }
})());