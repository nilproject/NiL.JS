try {
    eval('(function(){})(,1);');
    console.log("empty func argument fail #1");
}
catch (e) {
    if (!(e instanceof SyntaxError))
        console.log("empty func argument fail #1.1");
}
try {
    eval('(function(){})(1,,1);');
    console.log("empty func argument fail #2");
}
catch (e) {
    if (!(e instanceof SyntaxError))
        console.log("empty func argument fail #2.1");
}
try {
    eval('(function(){})(1,);');
    console.log("empty func argument fail #3");
}
catch (e) {
    if (!(e instanceof SyntaxError))
        console.log("empty func argument fail #3.1");
}

if (new Date(Date.parse("2015-10-27T01:01:01.000Z")).toString() != new Date(Date.parse("2015-10-27T01:01:01Z")).toString())
    console.log("Incorrect handling seconds")

if (new Date(Date.parse("2015-10-27T01:02:03.004Z")).toString() == "Invalid date")
    console.log("Date.parse broken");