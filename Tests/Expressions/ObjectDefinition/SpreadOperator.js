var obj = {
    ...{ a: 1, b: 2 },
    c: 'c'
};

Debug.asserta(() => obj.a == 1 && obj.b == 2 && obj.c == 'c');