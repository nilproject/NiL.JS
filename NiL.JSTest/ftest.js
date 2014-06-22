
var a = [1, 2, 3]
a.x = 10;
var d = delete a[1]
if (d === true && a[1] === undefined)
    console.log(true);
