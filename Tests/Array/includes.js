var array = [1, 2, 3, NaN, 5];

if (array.includes(3) !== true)
    console.log("Incorrect result #1");

if (array.includes(-1) !== false)
    console.log("Incorrect result #2");

if (array.includes('3') !== false)
    console.log("Incorrect result #3");

if (array.includes(NaN) !== true)
    console.log("Incorrect result #4");