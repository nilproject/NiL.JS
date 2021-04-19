try {
    (function f(){
        f();
    })();
}
catch(e) {
    /* WoW! I caugth stack owerflow! I'm amazing! */
}

try {
   var s = ' ';
   for (;;) s += s;
} catch (e) {
   /* Max string length is 2^31 - 1 */
}