var FACTORY = Object.prototype.toString;

try {
    instance = new FACTORY;
    $FAIL('#1: Object.prototype.toString can\'t be used as a constructor');
} catch (e) {
    $PRINT(e);
}