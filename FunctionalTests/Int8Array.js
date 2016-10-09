(function (T) {
    var array = T(256);
    for (var i = 0; i < array.length; i++)
        array[i] = i;
    assert(array.length === 256, "array.length === 256 " + T.name);
    for (var i = 0; i < array.length; i++) {
        assert((i < array.length >> 1 ? i : (i - 256)) === array[i], i + " " + T.name);
    }
})(Int8Array);

(function (T) {
    var array = T(256);
    for (var i = 0; i < array.byteLength; i++)
        array[i] = i;
    assert(array.byteLength === 256, "array.length === 256 " + T.name);
    for (var i = 0; i < array.byteLength; i++) {
        assert(i === array[i], i + " " + T.name);
    }
})(ArrayBuffer);

(function (T) {
    var array = T(512);
    for (var i = 0; i < array.length; i++)
        array[i] = i - 128;
    assert(array.length === 512, "array.length === 512 " + T.name);
    for (var i = 0; i < array.length; i++) {
        assert(Math.min(255, Math.max(0, i - 128)) === array[i], i + " " + T.name);
    }
})(Uint8ClampedArray);

(function (T) {
    var array = T(256);
    for (var i = 0; i < array.length; i++)
        array[i] = i;
    assert(array.length === 256, "array.length === 256 " + T.name);
    for (var i = 0; i < array.length; i++) {
        assert(i === array[i], i + " " + T.name);
    }
    array = array.subarray(1);
    assert(array.length === 255, "array.length === 255 " + T.name);
    for (var i = 0; i < array.length; i++) {
        assert((i + 1) === array[i], i + " " + T.name);
    }
    array = array.subarray(1, 10);
    assert(array.length === 9, "array.length === 9 " + T.name);
    for (var i = 0; i < array.length; i++) {
        assert((i + 2) === array[i], i + " " + T.name);
    }
})(Uint8Array);

(function (T) {
    var array = T([0, 1, 2, 3, 4]);
    assert(array.length === 5, "array.length === 5 " + T.name);
    for (var i = 0; i < array.length; i++) {
        assert(i === array[i], i + " " + T.name + " from array");
    }
})(Uint8Array);

(function (T) {
    var ab = ArrayBuffer(5);
    for (var i = 0; i < 5; i++) {
        ab[i] = i;
    }
    var array = T(ab);
    assert(array.length === 5, "array.length === 5 " + T.name);
    for (var i = 0; i < array.length; i++) {
        assert(i === array[i], i + " " + T.name + " from ArrayBuffer 1");
    }
    array = T(ab, 1);
    assert(array.length === 4, "array.length === 4 " + T.name);
    for (var i = 0; i < array.length; i++) {
        assert((i + 1) === array[i], i + " " + T.name + " from ArrayBuffer 2");
    }
    array = T(ab, 1, 2);
    assert(array.length === 2, "array.length === 2 " + T.name);
    for (var i = 0; i < array.length; i++) {
        assert((i + 1) === array[i], i + " " + T.name + " from ArrayBuffer 3");
    }
    assert(array[2] === undefined, "array[2] === undefined");
})(Uint8Array);

function assert(condition, text) {
    if (!condition)
        console.log(text);
}