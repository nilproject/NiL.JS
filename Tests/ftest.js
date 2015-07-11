console.log(function () {
    var $ERROR = console.log;
    x = []; x[0] = 0; x[3] = 3; x.shift();
    return x[0] == undefined;
}());