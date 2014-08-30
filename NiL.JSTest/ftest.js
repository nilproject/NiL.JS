_for: for (var j = 0; ; j++) {
    for (var i in [, 1, 2]) {
        if (!j)
            continue _for;
        break;
    }
    break;
}
console.log(["fail", "pass"][j]);