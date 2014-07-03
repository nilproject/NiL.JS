$ERROR = console.log;

console.log((function () {
    //CHECK#1
    if (typeof Object.prototype.propertyIsEnumerable !== "function") {
        $ERROR('#1: propertyIsEnumerable method is defined');
    }

    var proto = { rootprop: "avis" };

    function AVISFACTORY(name) { this.name = name };

    AVISFACTORY.prototype = proto;

    var seagull = new AVISFACTORY("seagull");

    //CHECK#2
    if (typeof seagull.propertyIsEnumerable !== "function") {
        $ERROR('#2: propertyIsEnumerable method is accessed');
    }

    //CHECK#3
    if (!(seagull.propertyIsEnumerable("name"))) {
        $ERROR('#3: propertyIsEnumerable method works properly');
    }

    //CHECK#4
    if (seagull.propertyIsEnumerable("rootprop")) {
        $ERROR('#4: propertyIsEnumerable method does not consider objects in the prototype chain');
    }
})());