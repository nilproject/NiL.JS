function __FACTORY() { };
__FACTORY.prototype = 1;

//////////////////////////////////////////////////////////////////////////////
//CHECK#1
if (typeof __FACTORY.prototype !== 'number') {
    $ERROR('#1: typeof __FACTORY.prototype === \'number\'. Actual: typeof __FACTORY.prototype ===' + (typeof __FACTORY.prototype));
}
//
//////////////////////////////////////////////////////////////////////////////

var __device = new __FACTORY();

//////////////////////////////////////////////////////////////////////////////
//CHECK#2
if (!(Object.prototype.isPrototypeOf(__device))) {
    $ERROR('#2: Object.prototype.isPrototypeOf(__device) === true');
}