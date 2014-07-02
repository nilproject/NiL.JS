console.log((function () {
    return JSON.stringify({ prop: 1 }, function (k, v) { return undefined }) === undefined;
})());