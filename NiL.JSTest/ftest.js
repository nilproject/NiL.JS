function macro(a, b) {
    "macro"; // force macrofunction validation. SyntaxError if fail
    return a * b - a - b;
}

console.log(macro(3, 3));