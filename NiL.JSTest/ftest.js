var sqrt = Math.sqrt;

// Function Declarations
function square(x) {
    square.arguments
    x = +x;
    return +(x * x);
}

function diag(x, y) {
    x = +x;
    y = +y;
    return +sqrt(square(x) + square(y));
}

for (var i = 0; i < 1000000; i++)
    diag(2, 3);