console.log((function () {
    try {
        Object.create({}, {
            prop: false
        });
        return false;
    } catch (e) {
        return (e instanceof TypeError);
    }
})());