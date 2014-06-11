var __10_4_2_1_1_1 = "str";
(function testcase() {
    try {

        var _eval = eval;
        var __10_4_2_1_1_1 = "str1";
        console.log(_eval("__10_4_2_1_1_1"));
        console.log(eval("__10_4_2_1_1_1"));
    } finally {
        delete this.__10_4_2_1_1_1;
    }
})();