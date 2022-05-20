var obj = {
    a: 1,
    ["a"]: 2,
};

Debug.asserta(() => obj.a == 2);

obj = {
    ["a"]: 1,
    a: 2,
};

Debug.asserta(() => obj.a == 2);