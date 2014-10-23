var $ERROR = console.log;

console.log(function () {
    var str = new String("abc");
    str[5] = "de";

    var expResult = ["0", "1", "2", "length", "5"];

    var result = Object.getOwnPropertyNames(str);

    for (var i of result)
        console.log(i);
}());