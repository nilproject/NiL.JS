for (__prop in this) {
    if (__prop === "__declared__var")
        enumed = true;
}
if (!(enumed)) {
    $ERROR('#1: When using property attributes, {DontEnum} not used');
}
//
//////////////////////////////////////////////////////////////////////////////

var __declared__var;