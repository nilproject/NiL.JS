Number.prototype.isPrime = function () {
    var n = this | 0;
    for (var i = 2; i < n; i++)
    {
        if (n % i == 0)
            return false;
    }
    return true;
}
console.log(8..isPrime());