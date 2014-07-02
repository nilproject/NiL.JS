console.log((function () {
    if (function () { return "gnullunazzgnull" }().lastIndexOf(null) !== 11) {
        return false;
    }
    return true;
})());