var __proto = { phylum: "avis" };

function Robin() { this.name = "robin" };
Robin.prototype = __proto;
var __my__robin = new Robin;
if (!("phylum" in __my__robin)) {
    $ERROR('#2: var __proto={phylum:"avis"}; function Robin(){this.name="robin"}; Robin.prototype=__proto; var __my__robin = new Robin; "phylum" in __my__robin');
}
if (__my__robin.hasOwnProperty("phylum")) {
    $ERROR('#3: var __proto={phylum:"avis"}; function Robin(){this.name="robin"}; Robin.prototype=__proto; var __my__robin = new Robin; __my__robin.hasOwnProperty("phylum") === false. Actual: ' + (__my__robin.hasOwnProperty("phylum")));
}