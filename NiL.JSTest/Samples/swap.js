var a = 1,
    b = 2;
__swap(a, b); // because the arguments are passed by reference to an external function, this operator can exist
console.log("a = " + a); // 2
console.log("b = " + b); // 1