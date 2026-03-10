var someModule = (function () {
    var storage = { foo: 123, bar: 456 };

    function SomeType() { }

    SomeType.prototype = {
        doSomething: function () {
            var result = '',
                nums = [9, 8, 7]
                ;

            // Declares a `storage` variable twice, which leads to a global scope violation
            for (var i = 0, storage = undefined, storage = nums.length; i < storage; i++) {
                result += nums[i];
            }

            result += this.getItemFromStorage("foo");

            return result;
        },
        getItemFromStorage: function (key) {
            return storage[key];
        }
    };

    return new SomeType();
}());

someModule.doSomething();