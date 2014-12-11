try {
    for ((function () { throw "NoInExpression" })() ; ; (function () { throw "SecondExpression" })()) {
        throw "Statement";
    }
    $ERROR('#1: (function(){throw "NoInExpression"})() lead to throwing exception');
} catch (e) {
    if (e !== "NoInExpression") {
        $ERROR('#2: When for (ExpressionNoIn ;  ; Expression) Statement is evaluated NoInExpression evaluates first');
    }
}