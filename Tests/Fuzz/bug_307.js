function getDiscounts(items) {
    const result = [];

    const size = items.length;

    for (let index = 0; index < size; index++) {
        const item = items[index];
        result.push(item);
    }

    return result;
}

console.assert(getDiscounts([1, 2, 3, 4]) == "1,2,3,4");