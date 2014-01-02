var __proto = { phylum: "avis" };


//////////////////////////////////////////////////////////////////////////////
//CHECK#1
if (!("valueOf" in __proto)) {
    $ERROR('#1: var __proto={phylum:"avis"}; "valueOf" in __proto');
}
//
//////////////////////////////////////////////////////////////////////////////

function Robin() { this.name = "robin" };
Robin.prototype = __proto;

var __my__robin = new Robin;

//////////////////////////////////////////////////////////////////////////////
//CHECK#2
if (!("phylum" in __my__robin)) {
    $ERROR('#2: var __proto={phylum:"avis"}; function Robin(){this.name="robin"}; Robin.prototype=__proto; var __my__robin = new Robin; "phylum" in __my__robin');
}
//
//////////////////////////////////////////////////////////////////////////////

//////////////////////////////////////////////////////////////////////////////
//CHECK#3
if (__my__robin.hasOwnProperty("phylum")) {
    $ERROR('#3: var __proto={phylum:"avis"}; function Robin(){this.name="robin"}; Robin.prototype=__proto; var __my__robin = new Robin; __my__robin.hasOwnProperty("phylum") === false. Actual: ' + (__my__robin.hasOwnProperty("phylum")));
}