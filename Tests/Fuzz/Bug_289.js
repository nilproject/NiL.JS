let arr = [1, 2, 3, 4, 5];
arr.splice(1, 0, 6);

let expected = [1, 6, 2, 3, 4, 5];

if (arr.join() != expected.join())
    console.log(arr.join());

arr = [1, 2, 3, 4, 5, 6];
arr.unshift(-2, -1, 0);
if (arr.join() != [-2, -1, 0, 1, 2, 3, 4, 5, 6].join())
    console.log(arr);