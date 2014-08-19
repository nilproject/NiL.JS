console.log(function () {
    var obj = { "0.000001": 1 };

    var desc = Object.getOwnPropertyDescriptor(obj, 0.000001);

    return desc.value === 1;
}());