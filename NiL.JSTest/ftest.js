console.log(function () {
    var a = "123-456-789".split("-");
    console.log(a);
    console.log(a.join('-'));
    a[0] = a[1];
}());