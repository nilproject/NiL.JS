try {
    var __result = __func();
} catch (e) {
    $FAIL("#1: Function call can appears in the program before the FunctionDeclaration appears");
}
if (__result !== "SECOND") {
    $ERROR('#1.1: __result === "SECOND". Actual: __result ===' + __result);
}
//
//////////////////////////////////////////////////////////////////////////////

function __func() { return "FIRST"; };

//////////////////////////////////////////////////////////////////////////////
//CHECK#2
__result = __func();
if (__result !== "SECOND") {
    $ERROR('#2: __result === "SECOND". Actual: __result ===' + __result);
}
//
//////////////////////////////////////////////////////////////////////////////

function __func() { return "SECOND"; };