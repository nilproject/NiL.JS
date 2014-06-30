var str = 'ABC';
var strObj = new String('ABC');

////////////////////////////////////////////////////////////
// CHECK#1
if (str.constructor !== strObj.constructor) {
    $ERROR('#1: \'ABC\'.constructor === new String(\'ABC\').constructor');
}