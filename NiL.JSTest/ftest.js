if (!(Object.prototype.toString.hasOwnProperty('length'))) {
    $FAIL('#0: the Object.prototype.toString has length property.');
}


// CHECK#1
if (Object.prototype.toString.propertyIsEnumerable('length')) {
    $ERROR('#1: the Object.prototype.toString.length property has the attributes DontEnum');
}

// CHECK#2
for (var p in Object.prototype.toString) {
    if (p === "length")
        $ERROR('#2: the Object.prototype.toString.length property has the attributes DontEnum');
}