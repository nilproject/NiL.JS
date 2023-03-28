(function (T) {
    var array = T(65536);
    for (var i = 0; i < array.length; i++)
        array[i] = i;
    assert(array.length === 65536, "array.length === 65536 " + T.name);
    for (var i = 0; i < array.length; i++) {
        assert((i < array.length >> 1 ? i : (i - 65536)) === array[i], i + " " + T.name);
    }
})(Int16Array);

(function (T) {
    var array = T(65536);
    for (var i = 0; i < array.length; i++)
        array[i] = i;
    assert(array.length === 65536, "array.length === 65536 " + T.name);
    for (var i = 0; i < array.length; i++) {
        assert(i === array[i], i + " " + T.name);
    }
    array = array.subarray(1);
    assert(array.length === 65535, "array.length === 65535 " + T.name);
    for (var i = 0; i < array.length; i++) {
        assert((i + 1) === array[i], i + " " + T.name);
    }
    array = array.subarray(1, 10);
    assert(array.length === 9, "array.length === 9 " + T.name);
    for (var i = 0; i < array.length; i++) {
        assert((i + 2) === array[i], i + " " + T.name);
    }
})(Uint16Array);

(function (T) {
    var array = T([0, 1, 2, 3, 4]);
    assert(array.length === 5, "array.length === 5 " + T.name);
    for (var i = 0; i < array.length; i++) {
        assert(i === array[i], i + " " + T.name + " from array");
    }
})(Uint16Array);

(function (T) {
    var ab = ArrayBuffer(6);
    for (var i = 0; i < 6; i++) {
        ab[i] = i;
    }
    var array = T(ab);
    assert(array.length === 3, "array.length === 3 " + T.name);
    assert(0x0100 === array[0], 0 + " " + T.name + " from ArrayBuffer 2");
    assert(0x0302 === array[1], 1 + " " + T.name + " from ArrayBuffer 2");
    assert(0x0504 === array[2], 2 + " " + T.name + " from ArrayBuffer 2");

    array = T(ab, 2);
    assert(array.length === 2, "array.length === 2 " + T.name);

    assert(0x0302 === array[0], 0 + " " + T.name + " from ArrayBuffer 2");
    assert(0x0504 === array[1], 1 + " " + T.name + " from ArrayBuffer 2");

    array = T(ab, 2, 1);
    assert(array.length === 1, "array.length === 1 " + T.name + " (" + array.length + ")");
    assert(0x0302 === array[0], i + " " + T.name + " from ArrayBuffer 3");

    assert(array[1] === undefined, "array[2] === undefined");
})(Uint16Array);

function assert(condition, text) {
    if (!condition)
        console.log(text);
}