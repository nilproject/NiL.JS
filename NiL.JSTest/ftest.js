var a = 1;

var __obj = { a: 2 };

with (__obj) {
    console.log(a);
        
    result = __func();

    function __func() { return a; };
    console.log(result);
}

//////////////////////////////////////////////////////////////////////////////
//CHECK#1
if (result !== 1) {
    $ERROR('#1: function declaration inside of "with" statement is a fuction declaration inside of current execution context');
}