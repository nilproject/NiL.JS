console.log(this.test);

try {
    {
        var test = 1;
    }
    throw "";
}
catch (e) {
    function test() {
        console.log("in try..catch");
    }

    console.log(test());
}