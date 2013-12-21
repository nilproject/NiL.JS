first = 1;
last = 10;

for (var current = first; current <= last; current++) {
    var n = current, i, k, m, currpath = []
    for (i = 0; (n != 1) && (isNaN(cycle[n])) ; i++) {
        currpath[i] = n
        n = (n % 2) ? 3 * n + 1 : n / 2
    }
    if (-i <= 0) {
        if (n == 1) i++
        else {
            if (!isNaN(cycle[n])) {
                i = i + cycle[n]
            }
        }
        if (i > maxcycle) maxcycle = i
        for (k = 0; k < currpath.length; k++) {
            if (!isNaN(currpath[k]))
                cycle[currpath[k]] = i - k
        }
    }
}