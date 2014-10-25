var $ERROR = console.log;

console.log(function () {
    try {
        var s1 = new Number();
        console.log(typeof s1);
        debugger;
        s1.toString = Boolean.prototype.toString;
        var v1 = s1.toString();
        $ERROR('#1: Boolean.prototype.toString on not a Boolean object should throw TypeError');
    }
    catch (e) {
        if (!(e instanceof TypeError)) {
            $ERROR('#1: Boolean.prototype.toString on not a Boolean object should throw TypeError, not ' + e);
        }
    }

    //CHECK#1
    try {
        var s2 = new Number();
        s2.myToString = Boolean.prototype.toString;
        var v2 = s2.myToString();
        $ERROR('#2: Boolean.prototype.toString on not a Boolean object should throw TypeError');
    }
    catch (e) {
        if (!(e instanceof TypeError)) {
            $ERROR('#2: Boolean.prototype.toString on not a Boolean object should throw TypeError, not ' + e);
        }
    }
}());