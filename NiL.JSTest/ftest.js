console.log(function () {
    var aString = new String("test string");

    //////////////////////////////////////////////////////////////////////////////
    //CHECK#1
    if (aString.search(/String/i) !== 5) {
        $ERROR('#1: var aString = new String("test string"); aString.search(/String/i)=== 5. Actual: ' + aString.search(/String/i));
    }
}());